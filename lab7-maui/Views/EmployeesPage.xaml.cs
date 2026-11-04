using Microsoft.EntityFrameworkCore;
using ProjectContractsMaui.Data;
using ProjectContractsMaui.Models;

namespace ProjectContractsMaui.Views;

public partial class EmployeesPage : ContentPage
{
    private readonly IDbContextFactory<ContractsContext> dbFactory;

    public EmployeesPage(IDbContextFactory<ContractsContext> dbFactory)
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
        var q = db.Employees.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Search.Text))
            q = q.Where(e => EF.Functions.Like(e.LastName, Search.Text.Trim() + "%"));
        var items = await q.OrderBy(e => e.LastName)
            .Select(e => new EmployeeItem(e.Id, e.LastName + " " + e.FirstName + " " + e.MiddleName,
                e.Position.Title, e.Address, e.Phone, e.Participations.Count))
            .ToListAsync();
        List.ItemsSource = items;
        Count.Text = $"Знайдено: {items.Count}";
    }

    private async void OnSearch(object? sender, TextChangedEventArgs e) => await Load();

    private async void OnAdd(object? sender, EventArgs e) => await Shell.Current.GoToAsync("employee?id=0");

    private async void OnTap(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is EmployeeItem item)
            await Shell.Current.GoToAsync($"employee?id={item.Id}");
    }
}
