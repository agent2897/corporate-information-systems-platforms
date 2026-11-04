using System.ComponentModel.DataAnnotations;

namespace ProjectContractsPages.Models;

// Об'єкти передачі даних для Web API (без циклічних навігаційних посилань)

public record PositionDto(int Id, string Title, decimal Salary, int EmployeeCount);

public record EmployeeDto(int Id, string FullName, string Position, string Address, string? Phone, string? Characteristic);

public record ProjectDto(int Id, string Name, string? Description, string? Customer,
                         DateTime StartDate, DateTime EndDate, int DurationDays, int MemberCount);

public record ProjectInput(
    [Required(ErrorMessage = "Вкажіть назву"), StringLength(100)] string Name,
    [StringLength(1000)] string? Description,
    [StringLength(100)] string? Customer,
    DateTime StartDate,
    DateTime EndDate);

public record ParticipationDto(int EmployeeId, string Employee, string Position, int ProjectId, string Project, int DaysWorked);

public record ParticipationInput(
    [Range(1, int.MaxValue, ErrorMessage = "Оберіть працівника")] int EmployeeId,
    [Range(1, int.MaxValue, ErrorMessage = "Оберіть проект")] int ProjectId,
    [Range(1, 3650, ErrorMessage = "Кількість днів — від 1 до 3650")] int DaysWorked);
