using Microsoft.EntityFrameworkCore;

namespace ProjectContractsEf;

/// <summary>LINQ-запити до бази даних через EF Core. Результат — проекції з українськими назвами колонок.</summary>
public static class Repository
{
    public static IList<object> Employees()
    {
        using var db = new ContractsContext();
        return db.Employees.AsNoTracking()
            .Include(e => e.Position)
            .OrderBy(e => e.LastName)
            .AsEnumerable()
            .Select(e => (object)new
            {
                Код = e.Id, ПІБ = e.FullName, Посада = e.Position.Title,
                Домашня_адреса = e.Address, Телефон = e.Phone, Характеристика = e.Characteristic,
            }).ToList();
    }

    public static IList<object> Positions()
    {
        using var db = new ContractsContext();
        return db.Positions.AsNoTracking()
            .Select(p => new { p.Id, p.Title, p.Salary, Count = p.Employees.Count })
            .AsEnumerable()
            .OrderByDescending(p => p.Salary)
            .Select(p => (object)new { Код = p.Id, Посада = p.Title, Оклад_грн = p.Salary.ToString("N2"), Працівників = p.Count })
            .ToList();
    }

    public static IList<object> Projects()
    {
        using var db = new ContractsContext();
        return db.Projects.AsNoTracking()
            .OrderBy(p => p.StartDate)
            .Select(p => new { p.Id, p.Name, p.Customer, p.StartDate, p.EndDate, p.Description, Count = p.Participations.Count })
            .AsEnumerable()
            .Select(p => (object)new
            {
                Код = p.Id, Назва_проекту = p.Name, Замовник = p.Customer,
                Початок = p.StartDate.ToString("dd.MM.yyyy"), Закінчення = p.EndDate.ToString("dd.MM.yyyy"),
                Тривалість_дн = p.EndDate.DayNumber - p.StartDate.DayNumber,
                Учасників = p.Count, Опис_проекту = p.Description,
            }).ToList();
    }

    public static IList<(int Id, string Name)> ProjectList()
    {
        using var db = new ContractsContext();
        return db.Projects.OrderBy(p => p.Name).Select(p => new { p.Id, p.Name })
            .AsEnumerable().Select(p => (p.Id, p.Name)).ToList();
    }

    public static IList<object> Participation(int projectId)
    {
        using var db = new ContractsContext();
        return db.Participations.AsNoTracking()
            .Where(pa => projectId == 0 || pa.ProjectId == projectId)
            .Include(pa => pa.Project)
            .Include(pa => pa.Employee).ThenInclude(e => e.Position)
            .OrderBy(pa => pa.Project.Name).ThenBy(pa => pa.Employee.LastName)
            .AsEnumerable()
            .Select(pa => (object)new
            {
                Проект = pa.Project.Name, Працівник = pa.Employee.ShortName,
                Посада = pa.Employee.Position.Title, Днів_у_проекті = pa.DaysWorked,
                Оклад_грн = pa.Employee.Position.Salary.ToString("N2"),
            }).ToList();
    }

    public static IList<object> TwoProjects()
    {
        using var db = new ContractsContext();
        return db.Employees.AsNoTracking()
            .Where(e => e.Participations.Count >= 2)
            .Include(e => e.Position)
            .Include(e => e.Participations).ThenInclude(pa => pa.Project)
            .OrderBy(e => e.LastName)
            .AsEnumerable()
            .Select(e => (object)new
            {
                ПІБ = e.FullName, Посада = e.Position.Title,
                Проектів = e.Participations.Count,
                Усього_днів = e.Participations.Sum(pa => pa.DaysWorked),
                Проекти = string.Join("; ", e.Participations.Select(pa => pa.Project.Name)),
            }).ToList();
    }

    public static IList<object> Search(string text)
    {
        using var db = new ContractsContext();
        var pattern = text.Trim() + "%";
        return db.Employees.AsNoTracking()
            .Where(e => EF.Functions.Like(e.LastName, pattern))
            .Include(e => e.Position)
            .Include(e => e.Participations).ThenInclude(pa => pa.Project)
            .OrderBy(e => e.LastName)
            .AsEnumerable()
            .Select(e => (object)new
            {
                ПІБ = e.FullName, Посада = e.Position.Title, Домашня_адреса = e.Address,
                Участь_у_проектах = e.Participations.Count == 0 ? "—"
                    : string.Join("; ", e.Participations.Select(pa => $"{pa.Project.Name} ({pa.DaysWorked} дн.)")),
            }).ToList();
    }

    public static string Summary()
    {
        using var db = new ContractsContext();
        var fund = db.Employees.Select(e => e.Position.Salary).AsEnumerable().Sum();
        return $"Працівників: {db.Employees.Count()}   |   Посад: {db.Positions.Count()}   |   " +
               $"Проектів: {db.Projects.Count()}   |   Фонд окладів: {fund:N2} грн   |   EF Core {typeof(DbContext).Assembly.GetName().Version}";
    }
}
