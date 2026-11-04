using Microsoft.EntityFrameworkCore;
using ProjectContractsMaui.Data;
using ProjectContractsMaui.Models;

namespace ProjectContractsMaui.Views;

/// <summary>Створення (id = 0) та редагування працівника.</summary>
[QueryProperty(nameof(EmployeeId), "id")]
public partial class EmployeeEditPage : ContentPage
{
    private readonly IDbContextFactory<ContractsContext> dbFactory;
    private Employee employee = new();

    public int EmployeeId { get; set; }

    public EmployeeEditPage(IDbContextFactory<ContractsContext> dbFactory)
    {
        InitializeComponent();
        this.dbFactory = dbFactory;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await using var db = await dbFactory.CreateDbContextAsync();
        var positions = await db.Positions.OrderBy(p => p.Title).ToListAsync();
        Position.ItemsSource = positions;

        if (EmployeeId == 0)
        {
            Title = "Новий працівник";
            DeleteButton.IsVisible = false;
            return;
        }
        employee = await db.Employees.AsNoTracking().FirstAsync(e => e.Id == EmployeeId);
        Title = "Редагування";
        LastName.Text = employee.LastName; FirstName.Text = employee.FirstName; MiddleName.Text = employee.MiddleName;
        Phone.Text = employee.Phone; Address.Text = employee.Address; Characteristic.Text = employee.Characteristic;
        Position.SelectedItem = positions.FirstOrDefault(p => p.Id == employee.PositionId);

        var projects = await db.Participations.Where(p => p.EmployeeId == EmployeeId)
            .Select(p => p.Project.Name + " — " + p.DaysWorked + " дн.").ToListAsync();
        ProjectsInfo.Text = projects.Count == 0 ? "Не бере участі в проектах" : "Участь у проектах:\n" + string.Join("\n", projects);
    }

    private async void OnSave(object? sender, EventArgs e)
    {
        employee.LastName = LastName.Text?.Trim() ?? "";
        employee.FirstName = FirstName.Text?.Trim() ?? "";
        employee.MiddleName = MiddleName.Text?.Trim() ?? "";
        employee.PositionId = (Position.SelectedItem as Position)?.Id ?? 0;
        employee.Phone = string.IsNullOrWhiteSpace(Phone.Text) ? null : Phone.Text.Trim();
        employee.Address = Address.Text?.Trim() ?? "";
        employee.Characteristic = string.IsNullOrWhiteSpace(Characteristic.Text) ? null : Characteristic.Text.Trim();

        if (ModelValidator.Errors(employee) is { } errors)
        {
            Errors.Text = errors;
            Errors.IsVisible = true;
            return;
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        if (employee.Id == 0) db.Employees.Add(employee);
        else db.Employees.Update(employee);
        await db.SaveChangesAsync();
        await Shell.Current.GoToAsync("..");
    }

    private async void OnDelete(object? sender, EventArgs e)
    {
        if (!await DisplayAlert("Видалення", $"Видалити працівника {employee.FullName}? Записи про участь у проектах також буде видалено.", "Видалити", "Скасувати"))
            return;
        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Employees.Where(x => x.Id == employee.Id).ExecuteDeleteAsync();
        await Shell.Current.GoToAsync("..");
    }
}
