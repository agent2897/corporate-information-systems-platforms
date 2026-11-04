using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Data;
using ProjectContractsPages.Models;

namespace ProjectContractsPages.Controllers;

/// <summary>REST API проектів: GET/POST /api/projects, GET/PUT/DELETE /api/projects/{id}.</summary>
[ApiController, Route("api/projects")]
public class ProjectsController(ContractsContext db) : ControllerBase
{
    private static ProjectDto ToDto(Project p) => new(p.Id, p.Name, p.Description, p.Customer,
        p.StartDate, p.EndDate, (p.EndDate - p.StartDate).Days, p.Participations.Count);

    [HttpGet]
    public async Task<IEnumerable<ProjectDto>> GetAll() =>
        (await db.Projects.Include(p => p.Participations).OrderBy(p => p.StartDate).AsNoTracking().ToListAsync())
        .Select(ToDto);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Get(int id)
    {
        var p = await db.Projects.Include(x => x.Participations).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? NotFound() : ToDto(p);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(ProjectInput input)
    {
        if (await Validate(input, 0) is { } problem) return problem;
        var p = new Project();
        Apply(p, input);
        db.Projects.Add(p);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.Id }, ToDto(p));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProjectInput input)
    {
        var p = await db.Projects.FindAsync(id);
        if (p is null) return NotFound();
        if (await Validate(input, id) is { } problem) return problem;
        Apply(p, input);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Projects.FindAsync(id);
        if (p is null) return NotFound();
        db.Projects.Remove(p);          // участь видаляється каскадно
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static void Apply(Project p, ProjectInput i)
    {
        p.Name = i.Name.Trim(); p.Description = i.Description; p.Customer = i.Customer;
        p.StartDate = i.StartDate.Date; p.EndDate = i.EndDate.Date;
    }

    private async Task<ActionResult?> Validate(ProjectInput i, int id)
    {
        if (i.EndDate < i.StartDate)
            ModelState.AddModelError(nameof(i.EndDate), "Дата закінчення не може бути раніше дати початку");
        if (await db.Projects.AnyAsync(x => x.Name == i.Name.Trim() && x.Id != id))
            ModelState.AddModelError(nameof(i.Name), "Проект з такою назвою вже існує");
        return ModelState.IsValid ? null : ValidationProblem(ModelState);
    }
}
