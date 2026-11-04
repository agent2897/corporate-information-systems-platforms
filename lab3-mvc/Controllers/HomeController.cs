using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectContractsMvc.Data;

namespace ProjectContractsMvc.Controllers;

public class HomeController(ContractsContext db) : Controller
{
    /// <summary>Головна: зведена статистика та працівники, які працюють у двох і більше проектах.</summary>
    public async Task<IActionResult> Index()
    {
        ViewBag.Employees = await db.Employees.CountAsync();
        ViewBag.Positions = await db.Positions.CountAsync();
        ViewBag.Projects = await db.Projects.CountAsync();
        ViewBag.Fund = (await db.Employees.Select(e => e.Position.Salary).ToListAsync()).Sum();

        var multi = await db.Employees
            .Include(e => e.Position)
            .Include(e => e.Participations).ThenInclude(p => p.Project)
            .Where(e => e.Participations.Count >= 2)
            .OrderBy(e => e.LastName)
            .AsNoTracking().ToListAsync();
        return View(multi);
    }

    public IActionResult Error() => Content("Сталася помилка під час обробки запиту.");
}
