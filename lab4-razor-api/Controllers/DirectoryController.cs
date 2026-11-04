using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Data;
using ProjectContractsPages.Models;

namespace ProjectContractsPages.Controllers;

/// <summary>Довідкові GET-методи API: працівники та посади.</summary>
[ApiController, Route("api")]
public class DirectoryController(ContractsContext db) : ControllerBase
{
    [HttpGet("employees")]
    public async Task<IEnumerable<EmployeeDto>> Employees() =>
        await db.Employees.OrderBy(e => e.LastName)
            .Select(e => new EmployeeDto(e.Id, e.LastName + " " + e.FirstName + " " + e.MiddleName,
                e.Position.Title, e.Address, e.Phone, e.Characteristic))
            .ToListAsync();

    [HttpGet("employees/{id:int}")]
    public async Task<ActionResult<EmployeeDto>> Employee(int id)
    {
        var e = await db.Employees.Include(x => x.Position).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return e is null ? NotFound()
            : new EmployeeDto(e.Id, e.FullName, e.Position.Title, e.Address, e.Phone, e.Characteristic);
    }

    [HttpGet("positions")]
    public async Task<IEnumerable<PositionDto>> Positions() =>
        (await db.Positions.Select(p => new PositionDto(p.Id, p.Title, p.Salary, p.Employees.Count)).ToListAsync())
        .OrderByDescending(p => p.Salary);
}
