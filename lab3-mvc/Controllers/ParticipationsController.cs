using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectContractsMvc.Data;
using ProjectContractsMvc.Models;

namespace ProjectContractsMvc.Controllers;

/// <summary>Участь працівників у проектах (складений ключ EmployeeId + ProjectId).</summary>
public class ParticipationsController(ContractsContext db) : Controller
{
    public async Task<IActionResult> Index(int? projectId)
    {
        var q = db.Participations.Include(p => p.Employee).ThenInclude(e => e.Position)
            .Include(p => p.Project).AsNoTracking();
        if (projectId is > 0) q = q.Where(p => p.ProjectId == projectId);

        ViewBag.ProjectList = new SelectList(await db.Projects.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", projectId);
        return View(await q.OrderBy(p => p.Project.Name).ThenBy(p => p.Employee.LastName).ToListAsync());
    }

    public async Task<IActionResult> Create(int? projectId)
    {
        await FillLists();
        return View("Form", new Participation { ProjectId = projectId ?? 0, DaysWorked = 30 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Participation item)
    {
        if (await db.Participations.AnyAsync(p => p.EmployeeId == item.EmployeeId && p.ProjectId == item.ProjectId))
            ModelState.AddModelError(nameof(Participation.EmployeeId), "Цей працівник уже бере участь у вибраному проекті");
        if (!ModelState.IsValid) { await FillLists(); return View("Form", item); }
        db.Add(item);
        await db.SaveChangesAsync();
        TempData["Message"] = "Участь у проекті додано";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int employeeId, int projectId)
    {
        var item = await db.Participations.FindAsync(employeeId, projectId);
        if (item is null) return NotFound();
        await FillLists();
        ViewBag.IsEdit = true;
        return View("Form", item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Participation item)
    {
        if (!ModelState.IsValid) { await FillLists(); ViewBag.IsEdit = true; return View("Form", item); }
        db.Update(item);    // ключ не змінюється, оновлюється лише кількість днів
        await db.SaveChangesAsync();
        TempData["Message"] = "Кількість днів участі збережено";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int employeeId, int projectId)
    {
        var item = await db.Participations.Include(p => p.Employee).Include(p => p.Project)
            .AsNoTracking().FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.ProjectId == projectId);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int employeeId, int projectId)
    {
        var item = await db.Participations.FindAsync(employeeId, projectId);
        if (item is not null)
        {
            db.Participations.Remove(item);
            await db.SaveChangesAsync();
            TempData["Message"] = "Запис про участь видалено";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task FillLists()
    {
        ViewBag.EmployeeList = new SelectList(
            (await db.Employees.OrderBy(e => e.LastName).ToListAsync()).Select(e => new { e.Id, Name = e.FullName }), "Id", "Name");
        ViewBag.ProjectList = new SelectList(await db.Projects.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
    }
}
