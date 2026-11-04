using Microsoft.EntityFrameworkCore;
using ProjectContractsMaui.Data;
using ProjectContractsMaui.Models;

namespace ProjectContractsMaui.Views;

/// <summary>Сторінка проекту: учасники, додавання, зміна кількості днів, вилучення.</summary>
[QueryProperty(nameof(ProjectId), "id")]
public partial class ProjectDetailsPage : ContentPage
{
    private readonly IDbContextFactory<ContractsContext> dbFactory;

    public int ProjectId { get; set; }

    public ProjectDetailsPage(IDbContextFactory<ContractsContext> dbFactory)
    {
        InitializeComponent();
        this.dbFactory = dbFactory;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Load();
    }

    private async Task Load()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var p = await db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ProjectId);
        if (p is null) { await Shell.Current.GoToAsync(".."); return; }
        ProjectName.Text = p.Name;
        Customer.Text = p.Customer;
        Description.Text = p.Description;
        Period.Text = $"{p.StartDate:dd.MM.yyyy} — {p.EndDate:dd.MM.yyyy}";

        var members = await db.Participations.Where(x => x.ProjectId == ProjectId)
            .OrderBy(x => x.Employee.LastName)
            .Select(x => new MemberItem(x.EmployeeId,
                x.Employee.LastName + " " + x.Employee.FirstName + " " + x.Employee.MiddleName,
                x.Employee.Position.Title, x.DaysWorked))
            .ToListAsync();
        Members.ItemsSource = members;
        Totals.Text = $"Учасників: {members.Count} · людино-днів: {members.Sum(m => m.Days)}";
    }

    private async void OnEdit(object? sender, EventArgs e) => await Shell.Current.GoToAsync($"project-edit?id={ProjectId}");

    private async void OnAddMember(object? sender, EventArgs e)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var candidates = await db.Employees
            .Where(x => !x.Participations.Any(p => p.ProjectId == ProjectId))
            .OrderBy(x => x.LastName)
            .Select(x => new { x.Id, Name = x.LastName + " " + x.FirstName + " (" + x.Position.Title + ")" })
            .ToListAsync();
        if (candidates.Count == 0) { await DisplayAlert("Учасники", "Усі працівники вже беруть участь у проекті", "OK"); return; }

        var choice = await DisplayActionSheet("Оберіть працівника", "Скасувати", null, candidates.Select(c => c.Name).ToArray());
        var employee = candidates.FirstOrDefault(c => c.Name == choice);
        if (employee is null) return;
        if (await AskDays(employee.Name, 30) is not { } days) return;

        db.Participations.Add(new Participation { EmployeeId = employee.Id, ProjectId = ProjectId, DaysWorked = days });
        await db.SaveChangesAsync();
        await Load();
    }

    private async void OnMemberTap(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not MemberItem m) return;
        var action = await DisplayActionSheet(m.Employee, "Скасувати", "Вилучити з проекту", "Змінити кількість днів");
        await using var db = await dbFactory.CreateDbContextAsync();
        var row = db.Participations.Where(x => x.EmployeeId == m.EmployeeId && x.ProjectId == ProjectId);
        if (action == "Змінити кількість днів" && await AskDays(m.Employee, m.Days) is { } days)
            await row.ExecuteUpdateAsync(s => s.SetProperty(x => x.DaysWorked, days));
        else if (action == "Вилучити з проекту")
            await row.ExecuteDeleteAsync();
        await Load();
    }

    private async Task<int?> AskDays(string who, int current)
    {
        var text = await DisplayPromptAsync(who, "Кількість днів участі у проекті:", "Зберегти", "Скасувати",
            keyboard: Keyboard.Numeric, initialValue: current.ToString());
        if (text is null) return null;
        if (int.TryParse(text, out var days) && days is >= 1 and <= 3650) return days;
        await DisplayAlert("Помилка", "Кількість днів — ціле число від 1 до 3650", "OK");
        return null;
    }
}
