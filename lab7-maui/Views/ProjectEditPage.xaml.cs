using Microsoft.EntityFrameworkCore;
using ProjectContractsMaui.Data;
using ProjectContractsMaui.Models;

namespace ProjectContractsMaui.Views;

/// <summary>Створення (id = 0) та редагування проекту.</summary>
[QueryProperty(nameof(ProjectId), "id")]
public partial class ProjectEditPage : ContentPage
{
    private readonly IDbContextFactory<ContractsContext> dbFactory;
    private Project project = new();

    public int ProjectId { get; set; }

    public ProjectEditPage(IDbContextFactory<ContractsContext> dbFactory)
    {
        InitializeComponent();
        this.dbFactory = dbFactory;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (ProjectId == 0)
        {
            Title = "Новий проект";
            DeleteButton.IsVisible = false;
            StartDate.Date = DateTime.Today;
            EndDate.Date = DateTime.Today.AddMonths(3);
            return;
        }
        await using var db = await dbFactory.CreateDbContextAsync();
        project = await db.Projects.AsNoTracking().FirstAsync(p => p.Id == ProjectId);
        Title = "Редагування проекту";
        ProjectName.Text = project.Name; Customer.Text = project.Customer; Description.Text = project.Description;
        StartDate.Date = project.StartDate; EndDate.Date = project.EndDate;
    }

    private async void OnSave(object? sender, EventArgs e)
    {
        project.Name = ProjectName.Text?.Trim() ?? "";
        project.Customer = string.IsNullOrWhiteSpace(Customer.Text) ? null : Customer.Text.Trim();
        project.Description = string.IsNullOrWhiteSpace(Description.Text) ? null : Description.Text.Trim();
        project.StartDate = StartDate.Date;
        project.EndDate = EndDate.Date;

        await using var db = await dbFactory.CreateDbContextAsync();
        var errors = ModelValidator.Errors(project);
        if (errors is null && await db.Projects.AnyAsync(p => p.Name == project.Name && p.Id != project.Id))
            errors = $"• Проект «{project.Name}» вже існує";
        if (errors is not null) { Errors.Text = errors; Errors.IsVisible = true; return; }

        if (project.Id == 0) db.Projects.Add(project); else db.Projects.Update(project);
        await db.SaveChangesAsync();
        await Shell.Current.GoToAsync("..");
    }

    private async void OnDelete(object? sender, EventArgs e)
    {
        if (!await DisplayAlert("Видалення", $"Видалити проект «{project.Name}» разом із записами участі?", "Видалити", "Скасувати"))
            return;
        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Projects.Where(p => p.Id == project.Id).ExecuteDeleteAsync();
        await Shell.Current.GoToAsync("../..");   // сторінка проекту більше не існує — повертаємось до списку
    }
}
