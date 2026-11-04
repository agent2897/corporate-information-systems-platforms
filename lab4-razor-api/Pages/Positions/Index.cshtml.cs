using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Data;
using ProjectContractsPages.Models;

namespace ProjectContractsPages.Pages.Positions;

/// <summary>Одна сторінка з усіма операціями над посадами (обробники OnPostSave / OnPostDelete).</summary>
public class IndexModel(ContractsContext db) : PageModel
{
    public List<Position> Positions { get; private set; } = new();
    [BindProperty] public Position Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? editId)
    {
        if (editId is not null)
        {
            var p = await db.Positions.FindAsync(editId);
            if (p is null) return NotFound();
            Input = p;
        }
        await Load();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (await db.Positions.AnyAsync(x => x.Title == Input.Title && x.Id != Input.Id))
            ModelState.AddModelError("Input.Title", "Така посада вже існує");
        if (!ModelState.IsValid) { await Load(); return Page(); }

        if (Input.Id == 0) db.Positions.Add(Input);
        else db.Positions.Update(Input);
        await db.SaveChangesAsync();
        TempData["Message"] = $"Посаду «{Input.Title}» збережено";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var p = await db.Positions.Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return RedirectToPage();
        if (p.Employees.Count > 0)
            TempData["Error"] = $"Посаду «{p.Title}» не можна видалити: її займають {p.Employees.Count} працівн.";
        else
        {
            db.Positions.Remove(p);
            await db.SaveChangesAsync();
            TempData["Message"] = $"Посаду «{p.Title}» видалено";
        }
        return RedirectToPage();
    }

    private async Task Load() =>
        Positions = (await db.Positions.Include(p => p.Employees).AsNoTracking().ToListAsync())
            .OrderByDescending(p => p.Salary).ToList();
}
