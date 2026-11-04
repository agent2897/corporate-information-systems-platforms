using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectContractsMvc.Data;
using ProjectContractsMvc.Models;

namespace ProjectContractsMvc.Controllers;

public class EmployeesController(ContractsContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, int? positionId)
    {
        var q = db.Employees.Include(e => e.Position).Include(e => e.Participations).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(e => EF.Functions.Like(e.LastName, search.Trim() + "%"));
        if (positionId is > 0)
            q = q.Where(e => e.PositionId == positionId);

        ViewBag.Search = search;
        ViewBag.PositionList = new SelectList(await db.Positions.OrderBy(p => p.Title).ToListAsync(), "Id", "Title", positionId);
        return View(await q.OrderBy(e => e.LastName).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var e = await db.Employees.Include(x => x.Position)
            .Include(x => x.Participations).ThenInclude(p => p.Project)
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return e is null ? NotFound() : View(e);
    }

    public async Task<IActionResult> Create()
    {
        await FillPositions();
        return View("Form", new Employee());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (!ModelState.IsValid) { await FillPositions(); return View("Form", employee); }
        db.Add(employee);
        await db.SaveChangesAsync();
        TempData["Message"] = $"Працівника «{employee.FullName}» додано";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.Employees.FindAsync(id);
        if (e is null) return NotFound();
        await FillPositions();
        return View("Form", e);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.Id) return BadRequest();
        if (!ModelState.IsValid) { await FillPositions(); return View("Form", employee); }
        db.Update(employee);
        await db.SaveChangesAsync();
        TempData["Message"] = $"Дані працівника «{employee.FullName}» збережено";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var e = await db.Employees.Include(x => x.Position).Include(x => x.Participations)
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return e is null ? NotFound() : View(e);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var e = await db.Employees.FindAsync(id);
        if (e is not null)
        {
            db.Employees.Remove(e);   // участь у проектах видаляється каскадно
            await db.SaveChangesAsync();
            TempData["Message"] = $"Працівника «{e.FullName}» видалено";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task FillPositions() =>
        ViewBag.PositionList = new SelectList(await db.Positions.OrderBy(p => p.Title).ToListAsync(), "Id", "Title");
}
