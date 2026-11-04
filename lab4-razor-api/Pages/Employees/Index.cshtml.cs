using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Data;
using ProjectContractsPages.Models;

namespace ProjectContractsPages.Pages.Employees;

public class IndexModel(ContractsContext db) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? PositionId { get; set; }

    public List<Employee> Employees { get; private set; } = new();
    public SelectList PositionList { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        var q = db.Employees.Include(e => e.Position).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(Search))
            q = q.Where(e => EF.Functions.Like(e.LastName, Search.Trim() + "%"));
        if (PositionId is > 0)
            q = q.Where(e => e.PositionId == PositionId);
        Employees = await q.OrderBy(e => e.LastName).ToListAsync();
        PositionList = new SelectList(await db.Positions.OrderBy(p => p.Title).ToListAsync(), "Id", "Title");
    }
}
