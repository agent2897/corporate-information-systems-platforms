using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectContractsMvc.Data;
using ProjectContractsMvc.Models;

namespace ProjectContractsMvc.Controllers;

public class PositionsController(ContractsContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var list = await db.Positions.Include(p => p.Employees).AsNoTracking().ToListAsync();
        return View(list.OrderByDescending(p => p.Salary).ToList());   // SQLite не сортує decimal на сервері
    }

    public IActionResult Create() => View("Form", new Position());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Position position)
    {
        await CheckUnique(position);
        if (!ModelState.IsValid) return View("Form", position);
        db.Add(position);
        await db.SaveChangesAsync();
        TempData["Message"] = $"Посаду «{position.Title}» додано";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await db.Positions.FindAsync(id);
        return p is null ? NotFound() : View("Form", p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Position position)
    {
        if (id != position.Id) return BadRequest();
        await CheckUnique(position);
        if (!ModelState.IsValid) return View("Form", position);
        db.Update(position);
        await db.SaveChangesAsync();
        TempData["Message"] = $"Посаду «{position.Title}» збережено";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Positions.Include(x => x.Employees).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? NotFound() : View(p);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var p = await db.Positions.Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return RedirectToAction(nameof(Index));
        if (p.Employees.Count > 0)
        {
            TempData["Error"] = $"Посаду «{p.Title}» не можна видалити: її займають {p.Employees.Count} працівн.";
            return RedirectToAction(nameof(Index));
        }
        db.Positions.Remove(p);
        await db.SaveChangesAsync();
        TempData["Message"] = $"Посаду «{p.Title}» видалено";
        return RedirectToAction(nameof(Index));
    }

    private async Task CheckUnique(Position p)
    {
        if (await db.Positions.AnyAsync(x => x.Title == p.Title && x.Id != p.Id))
            ModelState.AddModelError(nameof(Position.Title), "Така посада вже існує");
    }
}
