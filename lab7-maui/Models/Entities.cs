using System.ComponentModel.DataAnnotations;

namespace ProjectContractsMaui.Models;

/// <summary>Посада та посадовий оклад.</summary>
public class Position
{
    public int Id { get; set; }

    [Display(Name = "Посада"), Required(ErrorMessage = "Вкажіть назву посади"), StringLength(100)]
    public string Title { get; set; } = "";

    [Display(Name = "Оклад, грн"), Range(1, 1_000_000, ErrorMessage = "Оклад має бути від 1 до 1 000 000 грн")]
    public decimal Salary { get; set; }

    public List<Employee> Employees { get; set; } = new();
}

/// <summary>Працівник.</summary>
public class Employee
{
    public int Id { get; set; }

    [Display(Name = "Прізвище"), Required(ErrorMessage = "Вкажіть прізвище"), StringLength(50)]
    public string LastName { get; set; } = "";

    [Display(Name = "Ім’я"), Required(ErrorMessage = "Вкажіть ім’я"), StringLength(50)]
    public string FirstName { get; set; } = "";

    [Display(Name = "По батькові"), Required(ErrorMessage = "Вкажіть по батькові"), StringLength(50)]
    public string MiddleName { get; set; } = "";

    [Display(Name = "Посада"), Range(1, int.MaxValue, ErrorMessage = "Оберіть посаду")]
    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;

    [Display(Name = "Домашня адреса"), Required(ErrorMessage = "Вкажіть адресу"), StringLength(200)]
    public string Address { get; set; } = "";

    [Display(Name = "Характеристика"), StringLength(500)]
    public string? Characteristic { get; set; }

    [Display(Name = "Телефон"), RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Формат: +380XXXXXXXXX")]
    public string? Phone { get; set; }

    public List<Participation> Participations { get; set; } = new();

    public string FullName => $"{LastName} {FirstName} {MiddleName}";
}

/// <summary>Проект.</summary>
public class Project : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Назва проекту"), Required(ErrorMessage = "Вкажіть назву"), StringLength(100)]
    public string Name { get; set; } = "";

    [Display(Name = "Опис проекту"), StringLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Дата початку"), DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Display(Name = "Дата закінчення"), DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);

    [Display(Name = "Замовник"), StringLength(100)]
    public string? Customer { get; set; }

    public List<Participation> Participations { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (EndDate < StartDate)
            yield return new ValidationResult("Дата закінчення не може бути раніше дати початку", new[] { nameof(EndDate) });
    }
}

/// <summary>Участь працівника в проекті (M:N з даними).</summary>
public class Participation
{
    [Display(Name = "Працівник"), Range(1, int.MaxValue, ErrorMessage = "Оберіть працівника")]
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    [Display(Name = "Проект"), Range(1, int.MaxValue, ErrorMessage = "Оберіть проект")]
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Display(Name = "Днів у проекті"), Range(1, 3650, ErrorMessage = "Кількість днів — від 1 до 3650")]
    public int DaysWorked { get; set; }
}
