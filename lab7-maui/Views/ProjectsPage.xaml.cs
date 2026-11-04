using Microsoft.EntityFrameworkCore;
using ProjectContractsMaui.Data;
using ProjectContractsMaui.Models;

namespace ProjectContractsMaui.Views;

public partial class ProjectsPage : ContentPage
{
    private readonly IDbContextFactory<ContractsContext> dbFactory;

    public ProjectsPage(IDbContextFactory<ContractsContext> dbFactory)
    {
        InitializeComponent();
        this.dbFactory = dbFactory;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await using var db = await dbFactory.CreateDbContextAsync();
        List.ItemsSource = await db.Projects.OrderBy(p => p.StartDate)
            .Select(p => new ProjectItem(p.Id, p.Name, p.Customer, p.Description, p.StartDate, p.EndDate,
                p.Participations.Count, p.Participations.Sum(x => x.DaysWorked)))
            .ToListAsync();
    }

    private async void OnAdd(object? sender, EventArgs e) => await Shell.Current.GoToAsync("project-edit?id=0");

    private async void OnTap(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is ProjectItem p)
            await Shell.Current.GoToAsync($"project?id={p.Id}");
    }
}
