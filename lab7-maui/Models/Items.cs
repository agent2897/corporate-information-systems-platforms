using System.ComponentModel.DataAnnotations;

namespace ProjectContractsMaui.Models;

// Записи для відображення в списках (проекції запитів EF Core)

public record EmployeeItem(int Id, string FullName, string Position, string Address, string? Phone, int Projects)
{
    public string Info => $"{Phone ?? "без телефону"} · проектів: {Projects}";
}

public record PositionItem(int Id, string Title, decimal Salary, int Employees)
{
    public string SalaryText => $"{Salary:N2} грн";
    public string Info => $"працівників: {Employees}";
}

public record ProjectItem(int Id, string Name, string? Customer, string? Description,
                          DateTime StartDate, DateTime EndDate, int Members, int Days)
{
    public string Period => $"{StartDate:dd.MM.yyyy} — {EndDate:dd.MM.yyyy} ({(EndDate - StartDate).Days} дн.)";
    public string Stats => $"учасників: {Members} · людино-днів: {Days}";
}

public record MemberItem(int EmployeeId, string Employee, string Position, int Days)
{
    public string DaysText => $"{Days} дн.";
}

/// <summary>Перевірка моделі за атрибутами DataAnnotations та IValidatableObject.</summary>
public static class ModelValidator
{
    public static string? Errors(object model)
    {
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true)
            ? null
            : string.Join("\n", results.Select(r => "• " + r.ErrorMessage));
    }
}
