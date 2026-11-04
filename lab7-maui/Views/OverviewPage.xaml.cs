using Microsoft.EntityFrameworkCore;
using ProjectContractsMaui.Data;

namespace ProjectContractsMaui.Views;

public partial class OverviewPage : ContentPage
{
    private readonly IDbContextFactory<ContractsContext> dbFactory;

    public OverviewPage(IDbContextFactory<ContractsContext> dbFactory)
    {
        InitializeComponent();
        this.dbFactory = dbFactory;
    }

    // Дані перечитуються щоразу при поверненні на вкладку
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await using var db = await dbFactory.CreateDbContextAsync();
        Employees.Text = (await db.Employees.CountAsync()).ToString();
        Positions.Text = (await db.Positions.CountAsync()).ToString();
        Projects.Text = (await db.Projects.CountAsync()).ToString();
        Fund.Text = (await db.Employees.Select(e => e.Position.Salary).ToListAsync()).Sum().ToString("N0");

        var multi = await db.Employees
            .Where(e => e.Participations.Count >= 2)
            .OrderBy(e => e.LastName)
            .Select(e => new
            {
                Name = e.LastName + " " + e.FirstName.Substring(0, 1) + ". " + e.MiddleName.Substring(0, 1) + ".",
                Position = e.Position.Title,
                Days = e.Participations.Sum(p => p.DaysWorked),
            }).ToListAsync();

        TwoProjects.Children.Clear();
        foreach (var m in multi)
            TwoProjects.Children.Add(new Grid
            {
                ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto) },
                Children =
                {
                    new Label { Text = $"{m.Name}\n{m.Position}", FontSize = 14 },
                    new Label { Text = $"{m.Days} дн.", FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center }.Column(1),
                },
            });
    }
}

internal static class GridExt
{
    public static T Column<T>(this T view, int column) where T : BindableObject
    {
        Grid.SetColumn(view, column);
        return view;
    }
}
