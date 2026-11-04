using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectContractsMvc.Data;
using ProjectContractsMvc.Models;

namespace ProjectContractsMvc.Controllers;

public class ProjectsController(ContractsContext db) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.Projects.Include(p => p.Participations).OrderBy(p => p.StartDate).AsNoTracking().ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var p = await db.Projects
            .Include(x => x.Participations).ThenInclude(pa => pa.Employee).ThenInclude(e => e.Position)
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? NotFound() : View(p);
    }

    public IActionResult Create() => View("Form", new Project());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        await CheckUnique(project);
        if (!ModelState.IsValid) return View("Form", project);
        db.Add(project);
        await db.SaveChangesAsync();
        TempData["Message"] = $"Проект «{project.Name}» додано";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await db.Projects.FindAsync(id);
        return p is null ? NotFound() : View("Form", p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.Id) return BadRequest();
        await CheckUnique(project);
        if (!ModelState.IsValid) return View("Form", project);
        db.Update(project);
        await db.SaveChangesAsync();
        TempData["Message"] = $"Проект «{project.Name}» збережено";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Projects.Include(x => x.Participations).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? NotFound() : View(p);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var p = await db.Projects.FindAsync(id);
        if (p is not null)
        {
            db.Projects.Remove(p);
            await db.SaveChangesAsync();
            TempData["Message"] = $"Проект «{p.Name}» видалено";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task CheckUnique(Project p)
    {
        if (await db.Projects.AnyAsync(x => x.Name == p.Name && x.Id != p.Id))
            ModelState.AddModelError(nameof(Project.Name), "Проект з такою назвою вже існує");
    }
}
