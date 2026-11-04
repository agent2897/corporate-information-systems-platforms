using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Data;
using ProjectContractsPages.Models;

namespace ProjectContractsPages.Pages.Employees;

public class DeleteModel(ContractsContext db) : PageModel
{
    public Employee Employee { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var e = await db.Employees.Include(x => x.Position).Include(x => x.Participations)
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NotFound();
        Employee = e;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var e = await db.Employees.FindAsync(id);
        if (e is not null)
        {
            db.Employees.Remove(e);
            await db.SaveChangesAsync();
            TempData["Message"] = $"Працівника «{e.FullName}» видалено";
        }
        return RedirectToPage("Index");
    }
}
