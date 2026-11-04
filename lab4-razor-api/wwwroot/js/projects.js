// Клієнт Web API для сторінки «Проекти»: CRUD проектів та учасників через fetch
const api = async (url, method = 'GET', body) => {
    const res = await fetch(url, {
        method,
        headers: body ? { 'Content-Type': 'application/json' } : {},
        body: body ? JSON.stringify(body) : undefined,
    });
    if (res.status === 400) throw { validation: (await res.json()).errors };
    if (!res.ok) throw new Error(`${method} ${url}: ${res.status}`);
    return res.status === 204 || res.status === 201 && !res.headers.get('content-type') ? null : res.json();
};

const $ = s => document.querySelector(s);
const fmt = d => new Date(d).toLocaleDateString('uk-UA');
const esc = s => (s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
const projectModal = new bootstrap.Modal('#projectModal');
const memberModal = new bootstrap.Modal('#memberModal');
let projects = [], selectedId = null, editingId = null;

function flash(text, type = 'success') {
    $('#alert').innerHTML = `<div class="alert alert-${type}"><i class="bi bi-info-circle"></i> ${esc(text)}</div>`;
}

function showErrors(form, errors) {
    form.querySelectorAll('.is-invalid').forEach(e => e.classList.remove('is-invalid'));
    for (const [field, messages] of Object.entries(errors || {})) {
        const fb = form.querySelector(`[data-for="${field}"]`);
        if (!fb) continue;
        fb.textContent = messages.join(' ');
        fb.previousElementSibling.classList.add('is-invalid');
    }
}

async function loadProjects() {
    projects = await api('/api/projects');
    if (!projects.some(p => p.id === selectedId)) selectedId = projects[0]?.id ?? null;
    $('#projectList').innerHTML = projects.map(p => `
        <div class="card p-3 ${p.id === selectedId ? 'border border-2 border-success' : ''}" role="button" data-id="${p.id}">
            <div class="d-flex justify-content-between">
                <h5 class="mb-1">${esc(p.name)}</h5>
                <div class="text-nowrap">
                    <button class="btn btn-sm btn-outline-primary" data-edit="${p.id}"><i class="bi bi-pencil"></i></button>
                    <button class="btn btn-sm btn-outline-danger" data-del="${p.id}"><i class="bi bi-trash"></i></button>
                </div>
            </div>
            <div class="text-muted small mb-1"><i class="bi bi-building"></i> ${esc(p.customer)}</div>
            <div class="small mb-2">${esc(p.description)}</div>
            <div><span class="badge text-bg-primary">${fmt(p.startDate)} — ${fmt(p.endDate)}</span>
                 <span class="badge text-bg-secondary">${p.durationDays} дн.</span>
                 <span class="badge text-bg-success">${p.memberCount} учасн.</span></div>
        </div>`).join('');
    await loadMembers();
}

async function loadMembers() {
    const p = projects.find(x => x.id === selectedId);
    $('#membersCard').hidden = !p;
    if (!p) return;
    $('#membersTitle').textContent = `Учасники: ${p.name}`;
    const rows = await api(`/api/participations?projectId=${p.id}`);
    $('#members').innerHTML = rows.map(r => `
        <tr><td>${esc(r.employee)}</td><td>${esc(r.position)}</td>
            <td><input type="number" min="1" class="form-control form-control-sm text-end" value="${r.daysWorked}" data-days="${r.employeeId}"></td>
            <td class="text-end"><button class="btn btn-sm btn-outline-danger" data-remove="${r.employeeId}"><i class="bi bi-x-lg"></i></button></td></tr>`).join('');
    $('#totalDays').textContent = rows.reduce((s, r) => s + r.daysWorked, 0);
}

function openProject(p) {
    editingId = p?.id ?? null;
    const f = $('#projectForm');
    showErrors(f, {});
    $('#projectModalTitle').textContent = p ? 'Редагування проекту' : 'Новий проект';
    const today = new Date().toISOString().slice(0, 10);
    f.name.value = p?.name ?? ''; f.customer.value = p?.customer ?? ''; f.description.value = p?.description ?? '';
    f.startDate.value = p?.startDate.slice(0, 10) ?? today; f.endDate.value = p?.endDate.slice(0, 10) ?? today;
    projectModal.show();
}

$('#btnNewProject').onclick = () => openProject(null);

$('#projectForm').onsubmit = async e => {
    e.preventDefault();
    const f = e.target;
    const body = { name: f.name.value, customer: f.customer.value, description: f.description.value,
                   startDate: f.startDate.value, endDate: f.endDate.value };
    try {
        if (editingId) await api(`/api/projects/${editingId}`, 'PUT', body);
        else selectedId = (await api('/api/projects', 'POST', body)).id;
        projectModal.hide();
        flash(editingId ? `Проект «${body.name}» оновлено (PUT)` : `Проект «${body.name}» створено (POST)`);
        await loadProjects();
    } catch (err) { err.validation ? showErrors(f, err.validation) : flash(err.message, 'danger'); }
};

$('#projectList').onclick = async e => {
    const edit = e.target.closest('[data-edit]'), del = e.target.closest('[data-del]'), card = e.target.closest('[data-id]');
    if (edit) return openProject(projects.find(p => p.id == edit.dataset.edit));
    if (del) {
        const p = projects.find(x => x.id == del.dataset.del);
        if (!confirm(`Видалити проект «${p.name}» разом з ${p.memberCount} записами участі?`)) return;
        await api(`/api/projects/${p.id}`, 'DELETE');
        flash(`Проект «${p.name}» видалено (DELETE)`);
        return loadProjects();
    }
    if (card) { selectedId = +card.dataset.id; await loadProjects(); }
};

$('#btnAddMember').onclick = async () => {
    const [all, current] = await Promise.all([api('/api/employees'), api(`/api/participations?projectId=${selectedId}`)]);
    const taken = new Set(current.map(c => c.employeeId));
    const f = $('#memberForm');
    showErrors(f, {});
    f.employeeId.innerHTML = '<option value="0">— оберіть працівника —</option>' +
        all.filter(e => !taken.has(e.id)).map(e => `<option value="${e.id}">${esc(e.fullName)} (${esc(e.position)})</option>`).join('');
    memberModal.show();
};

$('#memberForm').onsubmit = async e => {
    e.preventDefault();
    const f = e.target;
    try {
        await api('/api/participations', 'POST',
            { employeeId: +f.employeeId.value, projectId: selectedId, daysWorked: +f.daysWorked.value });
        memberModal.hide();
        flash('Учасника додано (POST /api/participations)');
        await loadProjects();
    } catch (err) { err.validation ? showErrors(f, err.validation) : flash(err.message, 'danger'); }
};

$('#members').onchange = async e => {
    const input = e.target.closest('[data-days]');
    if (!input) return;
    await api(`/api/participations/${input.dataset.days}/${selectedId}`, 'PUT',
        { employeeId: +input.dataset.days, projectId: selectedId, daysWorked: +input.value });
    flash('Кількість днів оновлено (PUT)');
    await loadMembers();
};

$('#members').onclick = async e => {
    const btn = e.target.closest('[data-remove]');
    if (!btn) return;
    await api(`/api/participations/${btn.dataset.remove}/${selectedId}`, 'DELETE');
    flash('Учасника вилучено з проекту (DELETE)');
    await loadProjects();
};

loadProjects().catch(err => flash(err.message, 'danger'));
