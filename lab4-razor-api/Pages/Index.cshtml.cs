using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Data;
using ProjectContractsPages.Models;

namespace ProjectContractsPages.Pages;

public class IndexModel(ContractsContext db) : PageModel
{
    public int EmployeeCount { get; private set; }
    public int PositionCount { get; private set; }
    public int ProjectCount { get; private set; }
    public decimal Fund { get; private set; }
    public List<Employee> MultiProject { get; private set; } = new();

    public async Task OnGetAsync()
    {
        EmployeeCount = await db.Employees.CountAsync();
        PositionCount = await db.Positions.CountAsync();
        ProjectCount = await db.Projects.CountAsync();
        Fund = (await db.Employees.Select(e => e.Position.Salary).ToListAsync()).Sum();
        MultiProject = await db.Employees
            .Include(e => e.Position)
            .Include(e => e.Participations).ThenInclude(p => p.Project)
            .Where(e => e.Participations.Count >= 2)
            .OrderBy(e => e.LastName).AsNoTracking().ToListAsync();
    }
}
