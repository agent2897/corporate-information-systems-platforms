using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjectContractsSignalR.Data;
using ProjectContractsSignalR.Models;

namespace ProjectContractsSignalR.Hubs;

/// <summary>
/// SignalR-хаб: усі операції з даними (читання, створення, зміна, видалення) виконуються через
/// виклики методів хаба по WebSocket. Після кожної зміни всі клієнти отримують DataChanged.
/// </summary>
public class ContractsHub(ContractsContext db) : Hub<IContractsClient>
{
    private static int online;
    private static readonly System.Globalization.CultureInfo Uk = new("uk-UA");

    private string Author => Context.GetHttpContext()?.Request.Query["user"].ToString() is { Length: > 0 } u ? u : "Гість";

    public override async Task OnConnectedAsync() => await Clients.All.OnlineCount(Interlocked.Increment(ref online));

    public override async Task OnDisconnectedAsync(Exception? e) => await Clients.All.OnlineCount(Interlocked.Decrement(ref online));

    // ---------- Зведення ----------
    public async Task<object> GetSummary() => new
    {
        employees = await db.Employees.CountAsync(),
        positions = await db.Positions.CountAsync(),
        projects = await db.Projects.CountAsync(),
        fund = (await db.Employees.Select(e => e.Position.Salary).ToListAsync()).Sum(),
        inTwoProjects = await db.Employees.Where(e => e.Participations.Count >= 2)
            .OrderBy(e => e.LastName).Select(e => e.LastName + " " + e.FirstName.Substring(0, 1) + ".").ToListAsync(),
    };

    // ---------- Посади ----------
    public async Task<List<object>> GetPositions() =>
        (await db.Positions.Select(p => new { p.Id, p.Title, p.Salary, employees = p.Employees.Count }).ToListAsync())
        .OrderByDescending(p => p.Salary).Cast<object>().ToList();

    public async Task SavePosition(Position p)
    {
        Validate(p);
        if (await db.Positions.AnyAsync(x => x.Title == p.Title && x.Id != p.Id))
            throw new HubException($"Посада «{p.Title}» вже існує");
        await Upsert(p, p.Id);
        await Notify("positions", $"посаду «{p.Title}» збережено (оклад {p.Salary.ToString("N0", Uk)} грн)");
    }

    public async Task DeletePosition(int id)
    {
        var p = await db.Positions.Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new HubException("Посаду не знайдено");
        if (p.Employees.Count > 0)
            throw new HubException($"Посаду «{p.Title}» не можна видалити: її займають {p.Employees.Count} працівн.");
        db.Positions.Remove(p);
        await db.SaveChangesAsync();
        await Notify("positions", $"посаду «{p.Title}» видалено");
    }

    // ---------- Працівники ----------
    public async Task<List<object>> GetEmployees(string? search)
    {
        var q = db.Employees.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(e => EF.Functions.Like(e.LastName, search.Trim() + "%"));
        return await q.OrderBy(e => e.LastName).Select(e => (object)new
        {
            e.Id, e.LastName, e.FirstName, e.MiddleName, e.PositionId, position = e.Position.Title,
            e.Address, e.Phone, e.Characteristic, projects = e.Participations.Count,
        }).ToListAsync();
    }

    public async Task SaveEmployee(Employee e)
    {
        Validate(e);
        if (!await db.Positions.AnyAsync(p => p.Id == e.PositionId)) throw new HubException("Оберіть посаду");
        await Upsert(e, e.Id);
        await Notify("employees", $"працівника «{e.FullName}» збережено");
    }

    public async Task DeleteEmployee(int id)
    {
        var e = await db.Employees.FindAsync(id) ?? throw new HubException("Працівника не знайдено");
        db.Employees.Remove(e);                // записи участі видаляються каскадно
        await db.SaveChangesAsync();
        await Notify("employees", $"працівника «{e.FullName}» видалено");
    }

    // ---------- Проекти ----------
    public async Task<List<object>> GetProjects() =>
        await db.Projects.OrderBy(p => p.StartDate).Select(p => (object)new
        {
            p.Id, p.Name, p.Customer, p.Description, p.StartDate, p.EndDate,
            members = p.Participations.Count, days = p.Participations.Sum(x => x.DaysWorked),
        }).ToListAsync();

    public async Task SaveProject(Project p)
    {
        Validate(p);
        if (await db.Projects.AnyAsync(x => x.Name == p.Name && x.Id != p.Id))
            throw new HubException($"Проект «{p.Name}» вже існує");
        await Upsert(p, p.Id);
        await Notify("projects", $"проект «{p.Name}» збережено");
    }

    public async Task DeleteProject(int id)
    {
        var p = await db.Projects.FindAsync(id) ?? throw new HubException("Проект не знайдено");
        db.Projects.Remove(p);
        await db.SaveChangesAsync();
        await Notify("projects", $"проект «{p.Name}» видалено");
    }

    // ---------- Участь у проектах ----------
    public async Task<List<object>> GetParticipants(int projectId) =>
        await db.Participations.Where(x => x.ProjectId == projectId).OrderBy(x => x.Employee.LastName)
            .Select(x => (object)new
            {
                x.EmployeeId, x.ProjectId, x.DaysWorked,
                employee = x.Employee.LastName + " " + x.Employee.FirstName + " " + x.Employee.MiddleName,
                position = x.Employee.Position.Title,
            }).ToListAsync();

    public async Task SaveParticipation(Participation item)
    {
        Validate(item);
        var existing = await db.Participations.FindAsync(item.EmployeeId, item.ProjectId);
        if (existing is null) db.Participations.Add(item);
        else existing.DaysWorked = item.DaysWorked;
        await db.SaveChangesAsync();
        var who = await db.Employees.Where(e => e.Id == item.EmployeeId).Select(e => e.LastName).FirstAsync();
        var proj = await db.Projects.Where(p => p.Id == item.ProjectId).Select(p => p.Name).FirstAsync();
        await Notify("participation", existing is null
            ? $"{who} доданий(а) до проекту «{proj}» ({item.DaysWorked} дн.)"
            : $"{who}: у проекті «{proj}» тепер {item.DaysWorked} дн.");
    }

    public async Task RemoveParticipation(int employeeId, int projectId)
    {
        var item = await db.Participations.Include(x => x.Employee).Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.ProjectId == projectId)
            ?? throw new HubException("Запис не знайдено");
        db.Participations.Remove(item);
        await db.SaveChangesAsync();
        await Notify("participation", $"{item.Employee.LastName} вилучений(а) з проекту «{item.Project.Name}»");
    }

    // ---------- Допоміжні ----------
    private static void Validate(object model)
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true))
            throw new HubException(string.Join("; ", results.Select(r => r.ErrorMessage)));
    }

    private async Task Upsert<T>(T entity, int id) where T : class
    {
        if (id == 0) db.Add(entity); else db.Update(entity);
        await db.SaveChangesAsync();
    }

    private Task Notify(string entity, string message) =>
        Clients.All.DataChanged(entity, message, Author, Context.ConnectionId);
}
