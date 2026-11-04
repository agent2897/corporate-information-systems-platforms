namespace ProjectContractsEf.Models;

/// <summary>Посада та посадовий оклад.</summary>
public class Position
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Salary { get; set; }
    public List<Employee> Employees { get; set; } = new();
}

/// <summary>Працівник.</summary>
public class Employee
{
    public int Id { get; set; }
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;
    public string Address { get; set; } = "";
    public string? Characteristic { get; set; }
    public string? Phone { get; set; }
    public List<Participation> Participations { get; set; } = new();

    public string FullName => $"{LastName} {FirstName} {MiddleName}";
    public string ShortName => $"{LastName} {FirstName[0]}. {MiddleName[0]}.";
}

/// <summary>Проект.</summary>
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Customer { get; set; }
    public List<Participation> Participations { get; set; } = new();
}

/// <summary>Участь працівника в проекті (зв'язок M:N з даними).</summary>
public class Participation
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public int DaysWorked { get; set; }
}
