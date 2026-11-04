using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Data;
using ProjectContractsPages.Models;

namespace ProjectContractsPages.Controllers;

/// <summary>REST API участі: /api/participations?projectId=, /api/participations/{employeeId}/{projectId}.</summary>
[ApiController, Route("api/participations")]
public class ParticipationsController(ContractsContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<ParticipationDto>> GetAll(int? projectId)
    {
        var q = db.Participations.AsNoTracking();
        if (projectId is > 0) q = q.Where(p => p.ProjectId == projectId);
        return await q.OrderBy(p => p.Employee.LastName)
            .Select(p => new ParticipationDto(p.EmployeeId,
                p.Employee.LastName + " " + p.Employee.FirstName + " " + p.Employee.MiddleName,
                p.Employee.Position.Title, p.ProjectId, p.Project.Name, p.DaysWorked))
            .ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ParticipationInput input)
    {
        if (!await db.Employees.AnyAsync(e => e.Id == input.EmployeeId))
            ModelState.AddModelError(nameof(input.EmployeeId), "Працівника не знайдено");
        if (!await db.Projects.AnyAsync(p => p.Id == input.ProjectId))
            ModelState.AddModelError(nameof(input.ProjectId), "Проект не знайдено");
        if (await db.Participations.AnyAsync(p => p.EmployeeId == input.EmployeeId && p.ProjectId == input.ProjectId))
            ModelState.AddModelError(nameof(input.EmployeeId), "Цей працівник уже бере участь у проекті");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        db.Participations.Add(new Participation
            { EmployeeId = input.EmployeeId, ProjectId = input.ProjectId, DaysWorked = input.DaysWorked });
        await db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("{employeeId:int}/{projectId:int}")]
    public async Task<IActionResult> Update(int employeeId, int projectId, ParticipationInput input)
    {
        var p = await db.Participations.FindAsync(employeeId, projectId);
        if (p is null) return NotFound();
        p.DaysWorked = input.DaysWorked;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{employeeId:int}/{projectId:int}")]
    public async Task<IActionResult> Delete(int employeeId, int projectId)
    {
        var p = await db.Participations.FindAsync(employeeId, projectId);
        if (p is null) return NotFound();
        db.Participations.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
