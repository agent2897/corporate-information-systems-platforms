// Клієнт SignalR: усі дані читаються й змінюються викликами методів хаба (connection.invoke)
const user = new URLSearchParams(location.search).get('user') || 'Гість';
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/contracts?user=' + encodeURIComponent(user))
    .withAutomaticReconnect()
    .build();

const $ = s => document.querySelector(s);
const esc = s => String(s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
const money = v => Number(v).toLocaleString('uk-UA', { minimumFractionDigits: 2 });
const date = d => new Date(d).toLocaleDateString('uk-UA');
const employeeModal = new bootstrap.Modal('#employeeModal');
const projectModal = new bootstrap.Modal('#projectModal');
let positions = [], employees = [], projects = [], selectedProject = null, editingPositionId = null;
let lastChanged = null;   // підсвічування рядка, зміненого іншим користувачем

// ---------- Повідомлення ----------
function toast(text, type = 'success') {
    const el = document.createElement('div');
    el.className = `alert alert-${type} shadow-sm py-2`;
    el.innerHTML = text;
    $('#toasts').append(el);
    setTimeout(() => el.remove(), 4000);
}

async function call(method, ...args) {
    try { return await connection.invoke(method, ...args); }
    catch (err) {
        // HubException приходить як "An unexpected error ... HubException: <текст>"
        const msg = String(err.message).split('HubException: ').pop();
        throw new Error(msg);
    }
}

// ---------- Події від сервера ----------
connection.on('OnlineCount', n => $('#online').textContent = n);

connection.on('DataChanged', async (entity, message, author, connectionId) => {
    const mine = connectionId === connection.connectionId;
    const li = document.createElement('li');
    li.className = mine ? '' : 'other';
    li.innerHTML = `<div class="small text-muted">${new Date().toLocaleTimeString('uk-UA')} · ${esc(author)}</div>${esc(message)}`;
    const log = $('#log');
    if (log.firstElementChild?.classList.contains('text-muted')) log.innerHTML = '';
    log.prepend(li);
    if (!mine) { lastChanged = entity; toast(`<i class="bi bi-broadcast"></i> <b>${esc(author)}</b>: ${esc(message)}`, 'warning'); }
    await refresh();
});

// ---------- Завантаження ----------
async function refresh() {
    [positions, employees, projects] = await Promise.all([
        call('GetPositions'), call('GetEmployees', $('#search').value), call('GetProjects')]);
    renderSummary(await call('GetSummary'));
    renderEmployees(); renderPositions(); await renderProjects();
    lastChanged = null;
}

function renderSummary(s) {
    $('#summary').innerHTML = `
        <h6><i class="bi bi-database"></i> База даних</h6>
        <div class="d-flex justify-content-between"><span>Працівників</span><b>${s.employees}</b></div>
        <div class="d-flex justify-content-between"><span>Посад</span><b>${s.positions}</b></div>
        <div class="d-flex justify-content-between"><span>Проектів</span><b>${s.projects}</b></div>
        <div class="d-flex justify-content-between"><span>Фонд окладів</span><b>${money(s.fund)} грн</b></div>
        <div class="small text-muted mt-2">У двох проектах: ${s.inTwoProjects.map(esc).join(', ')}</div>`;
}

const flash = entity => lastChanged === entity ? 'flash' : '';

function renderEmployees() {
    $('#employees').innerHTML = employees.map(e => `
        <tr class="${flash('employees')}">
            <td>${esc(e.lastName)} ${esc(e.firstName)} ${esc(e.middleName)}</td>
            <td>${esc(e.position)}</td><td>${esc(e.address)}</td><td class="text-nowrap">${esc(e.phone)}</td>
            <td class="text-center">${e.projects}</td>
            <td class="text-end text-nowrap">
                <button class="btn btn-sm btn-outline-primary" data-edit-emp="${e.id}"><i class="bi bi-pencil"></i></button>
                <button class="btn btn-sm btn-outline-danger" data-del-emp="${e.id}"><i class="bi bi-trash"></i></button>
            </td></tr>`).join('') || '<tr><td colspan="6" class="text-center text-muted py-4">Нічого не знайдено</td></tr>';
}

function renderPositions() {
    $('#positions').innerHTML = positions.map(p => editingPositionId === p.id ? `
        <tr class="table-warning">
            <td><input class="form-control form-control-sm" id="editPosTitle" value="${esc(p.title)}"></td>
            <td><input type="number" step="0.01" class="form-control form-control-sm text-end" id="editPosSalary" value="${p.salary}"></td>
            <td class="text-center">${p.employees}</td>
            <td class="text-end text-nowrap">
                <button class="btn btn-sm btn-success" data-save-pos="${p.id}"><i class="bi bi-check-lg"></i></button>
                <button class="btn btn-sm btn-outline-secondary" data-cancel-pos><i class="bi bi-x-lg"></i></button></td></tr>` : `
        <tr class="${flash('positions')}">
            <td>${esc(p.title)}</td><td class="text-end">${money(p.salary)}</td><td class="text-center">${p.employees}</td>
            <td class="text-end text-nowrap">
                <button class="btn btn-sm btn-outline-primary" data-edit-pos="${p.id}"><i class="bi bi-pencil"></i></button>
                <button class="btn btn-sm btn-outline-danger" data-del-pos="${p.id}"><i class="bi bi-trash"></i></button></td></tr>`).join('');
}

async function renderProjects() {
    if (!projects.some(p => p.id === selectedProject)) selectedProject = projects[0]?.id ?? null;
    $('#projects').innerHTML = projects.map(p => `
        <div class="card p-3 ${p.id === selectedProject ? 'border border-2 border-info' : ''}" role="button" data-project="${p.id}">
            <div class="d-flex justify-content-between"><h6 class="mb-1">${esc(p.name)}</h6>
                <span class="text-nowrap"><button class="btn btn-sm btn-outline-primary" data-edit-proj="${p.id}"><i class="bi bi-pencil"></i></button>
                <button class="btn btn-sm btn-outline-danger" data-del-proj="${p.id}"><i class="bi bi-trash"></i></button></span></div>
            <div class="small text-muted">${esc(p.customer)}</div>
            <div class="small my-1">${esc(p.description)}</div>
            <div><span class="badge text-bg-primary">${date(p.startDate)} — ${date(p.endDate)}</span>
                 <span class="badge text-bg-success">${p.members} учасн.</span>
                 <span class="badge text-bg-secondary">${p.days} люд.-дн.</span></div>
        </div>`).join('');
    const proj = projects.find(p => p.id === selectedProject);
    if (!proj) { $('#members').innerHTML = ''; return; }
    $('#membersTitle').textContent = 'Учасники: ' + proj.name;
    const members = await call('GetParticipants', proj.id);
    $('#members').innerHTML = members.map(m => `
        <tr class="${flash('participation')}"><td>${esc(m.employee)}</td><td>${esc(m.position)}</td>
            <td><input type="number" class="form-control form-control-sm text-end" value="${m.daysWorked}" data-days="${m.employeeId}"></td>
            <td class="text-end"><button class="btn btn-sm btn-outline-danger" data-remove="${m.employeeId}"><i class="bi bi-x-lg"></i></button></td></tr>`).join('');
    const taken = new Set(members.map(m => m.employeeId));
    $('#newMember').innerHTML = '<option value="0">— додати працівника —</option>' + employees
        .filter(e => !taken.has(e.id)).map(e => `<option value="${e.id}">${esc(e.lastName)} ${esc(e.firstName)} (${esc(e.position)})</option>`).join('');
}

// ---------- Вкладки й пошук ----------
$('#tabs').onclick = e => {
    const tab = e.target.closest('[data-tab]')?.dataset.tab;
    if (!tab) return;
    document.querySelectorAll('[data-tab]').forEach(b => b.classList.toggle('active', b.dataset.tab === tab));
    document.querySelectorAll('[data-pane]').forEach(p => p.hidden = p.dataset.pane !== tab);
};
$('#search').oninput = async () => { employees = await call('GetEmployees', $('#search').value); renderEmployees(); };

// ---------- Працівники ----------
function openEmployee(emp) {
    const f = $('#employeeForm');
    f.dataset.id = emp?.id ?? 0;
    $('#employeeTitle').textContent = emp ? 'Редагування працівника' : 'Новий працівник';
    f.positionId.innerHTML = '<option value="0">— оберіть посаду —</option>' +
        positions.map(p => `<option value="${p.id}">${esc(p.title)}</option>`).join('');
    for (const k of ['lastName', 'firstName', 'middleName', 'phone', 'address', 'characteristic']) f[k].value = emp?.[k] ?? '';
    f.positionId.value = emp?.positionId ?? 0;
    f.querySelector('[data-error]').hidden = true;
    employeeModal.show();
}
$('#btnNewEmployee').onclick = () => openEmployee(null);
$('#employeeForm').onsubmit = async e => {
    e.preventDefault();
    const f = e.target;
    const dto = { id: +f.dataset.id, positionId: +f.positionId.value };
    for (const k of ['lastName', 'firstName', 'middleName', 'phone', 'address', 'characteristic']) dto[k] = f[k].value.trim() || (k === 'phone' || k === 'characteristic' ? null : '');
    try { await call('SaveEmployee', dto); employeeModal.hide(); }
    catch (err) { const box = f.querySelector('[data-error]'); box.textContent = err.message; box.hidden = false; }
};
$('#employees').onclick = async e => {
    const edit = e.target.closest('[data-edit-emp]'), del = e.target.closest('[data-del-emp]');
    if (edit) openEmployee(employees.find(x => x.id == edit.dataset.editEmp));
    if (del) {
        const emp = employees.find(x => x.id == del.dataset.delEmp);
        if (confirm(`Видалити працівника ${emp.lastName} ${emp.firstName}?`)) await call('DeleteEmployee', emp.id).catch(err => toast(err.message, 'danger'));
    }
};

// ---------- Посади ----------
$('#btnAddPosition').onclick = async () => {
    try {
        await call('SavePosition', { id: 0, title: $('#newPosTitle').value.trim(), salary: +$('#newPosSalary').value });
        $('#newPosTitle').value = ''; $('#newPosSalary').value = '';
    } catch (err) { toast(err.message, 'danger'); }
};
$('#positions').onclick = async e => {
    const t = e.target.closest('button');
    if (!t) return;
    if (t.dataset.editPos) { editingPositionId = +t.dataset.editPos; renderPositions(); }
    else if ('cancelPos' in t.dataset) { editingPositionId = null; renderPositions(); }
    else if (t.dataset.savePos) {
        try {
            await call('SavePosition', { id: +t.dataset.savePos, title: $('#editPosTitle').value.trim(), salary: +$('#editPosSalary').value });
            editingPositionId = null;
        } catch (err) { toast(err.message, 'danger'); }
    } else if (t.dataset.delPos) await call('DeletePosition', +t.dataset.delPos).catch(err => toast(err.message, 'danger'));
};

// ---------- Проекти ----------
function openProject(p) {
    const f = $('#projectForm');
    f.dataset.id = p?.id ?? 0;
    $('#projectTitle').textContent = p ? 'Редагування проекту' : 'Новий проект';
    const today = new Date().toISOString().slice(0, 10);
    f.name.value = p?.name ?? ''; f.customer.value = p?.customer ?? ''; f.description.value = p?.description ?? '';
    f.startDate.value = p?.startDate.slice(0, 10) ?? today; f.endDate.value = p?.endDate.slice(0, 10) ?? today;
    f.querySelector('[data-error]').hidden = true;
    projectModal.show();
}
$('#btnNewProject').onclick = () => openProject(null);
$('#projectForm').onsubmit = async e => {
    e.preventDefault();
    const f = e.target;
    try {
        await call('SaveProject', { id: +f.dataset.id, name: f.name.value.trim(), customer: f.customer.value.trim() || null,
            description: f.description.value.trim() || null, startDate: f.startDate.value, endDate: f.endDate.value });
        projectModal.hide();
    } catch (err) { const box = f.querySelector('[data-error]'); box.textContent = err.message; box.hidden = false; }
};
$('#projects').onclick = async e => {
    const edit = e.target.closest('[data-edit-proj]'), del = e.target.closest('[data-del-proj]'), card = e.target.closest('[data-project]');
    if (edit) return openProject(projects.find(p => p.id == edit.dataset.editProj));
    if (del) {
        const p = projects.find(x => x.id == del.dataset.delProj);
        if (confirm(`Видалити проект «${p.name}» разом з ${p.members} записами участі?`)) await call('DeleteProject', p.id);
        return;
    }
    if (card) { selectedProject = +card.dataset.project; await renderProjects(); }
};

// ---------- Участь ----------
$('#btnAddMember').onclick = () => call('SaveParticipation',
    { employeeId: +$('#newMember').value, projectId: selectedProject, daysWorked: +$('#newMemberDays').value })
    .catch(err => toast(err.message, 'danger'));
$('#members').onchange = e => {
    const input = e.target.closest('[data-days]');
    if (input) call('SaveParticipation', { employeeId: +input.dataset.days, projectId: selectedProject, daysWorked: +input.value })
        .catch(err => toast(err.message, 'danger'));
};
$('#members').onclick = e => {
    const b = e.target.closest('[data-remove]');
    if (b) call('RemoveParticipation', +b.dataset.remove, selectedProject);
};

// ---------- Старт ----------
const setStatus = (text, cls) => { $('#status').textContent = text; $('#status').className = 'badge ' + cls; };
connection.onreconnecting(() => setStatus('перепідключення…', 'text-bg-warning'));
connection.onreconnected(() => { setStatus('онлайн', 'text-bg-success'); refresh(); });
$('#me').textContent = user;
connection.start()
    .then(() => { setStatus('онлайн', 'text-bg-success'); return refresh(); })
    .catch(err => { setStatus('немає з’єднання', 'text-bg-danger'); console.error(err); });
