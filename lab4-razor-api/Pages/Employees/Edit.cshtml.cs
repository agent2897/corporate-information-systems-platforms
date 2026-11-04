using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Data;
using ProjectContractsPages.Models;

namespace ProjectContractsPages.Pages.Employees;

/// <summary>Створення (id відсутній) та редагування працівника.</summary>
public class EditModel(ContractsContext db) : PageModel
{
    [BindProperty] public Employee Employee { get; set; } = new();
    public SelectList PositionList { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is not null)
        {
            var e = await db.Employees.FindAsync(id);
            if (e is null) return NotFound();
            Employee = e;
        }
        await FillPositions();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await FillPositions(); return Page(); }

        if (Employee.Id == 0) db.Employees.Add(Employee);
        else db.Employees.Update(Employee);
        await db.SaveChangesAsync();

        TempData["Message"] = $"Працівника «{Employee.FullName}» збережено";
        return RedirectToPage("Index");
    }

    private async Task FillPositions() =>
        PositionList = new SelectList(await db.Positions.OrderBy(p => p.Title).ToListAsync(), "Id", "Title");
}
