using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProjectContractsMaui.Data;
using ProjectContractsMaui.Models;

namespace ProjectContractsMaui.Views;

/// <summary>Посади: додавання та зміна через діалоги, видалення лише незайнятих посад.</summary>
public partial class PositionsPage : ContentPage
{
    private readonly IDbContextFactory<ContractsContext> dbFactory;

    public PositionsPage(IDbContextFactory<ContractsContext> dbFactory)
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
        List.ItemsSource = (await db.Positions
                .Select(p => new PositionItem(p.Id, p.Title, p.Salary, p.Employees.Count)).ToListAsync())
            .OrderByDescending(p => p.Salary).ToList();
    }

    private async void OnAdd(object? sender, EventArgs e)
    {
        var title = await DisplayPromptAsync("Нова посада", "Назва посади:", "Далі", "Скасувати");
        if (string.IsNullOrWhiteSpace(title)) return;
        var salary = await AskSalary(title, null);
        if (salary is null) return;
        await Save(new Position { Title = title.Trim(), Salary = salary.Value });
    }

    private async void OnTap(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not PositionItem item) return;
        var action = await DisplayActionSheet(item.Title, "Скасувати", "Видалити", "Змінити оклад", "Перейменувати");
        switch (action)
        {
            case "Змінити оклад":
                if (await AskSalary(item.Title, item.Salary) is { } salary)
                    await Save(new Position { Id = item.Id, Title = item.Title, Salary = salary });
                break;
            case "Перейменувати":
                var title = await DisplayPromptAsync("Перейменування", "Нова назва посади:", "Зберегти", "Скасувати", initialValue: item.Title);
                if (!string.IsNullOrWhiteSpace(title))
                    await Save(new Position { Id = item.Id, Title = title.Trim(), Salary = item.Salary });
                break;
            case "Видалити":
                await Delete(item);
                break;
        }
    }

    private async Task<decimal?> AskSalary(string title, decimal? current)
    {
        var text = await DisplayPromptAsync(title, "Посадовий оклад, грн:", "Зберегти", "Скасувати",
            keyboard: Keyboard.Numeric, initialValue: current?.ToString("0.##", CultureInfo.InvariantCulture));
        if (text is null) return null;
        if (decimal.TryParse(text.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var v)) return v;
        await DisplayAlert("Помилка", "Введіть число", "OK");
        return null;
    }

    private async Task Save(Position p)
    {
        if (ModelValidator.Errors(p) is { } errors) { await DisplayAlert("Помилка", errors, "OK"); return; }
        await using var db = await dbFactory.CreateDbContextAsync();
        if (await db.Positions.AnyAsync(x => x.Title == p.Title && x.Id != p.Id))
        { await DisplayAlert("Помилка", $"Посада «{p.Title}» вже існує", "OK"); return; }
        if (p.Id == 0) db.Positions.Add(p); else db.Positions.Update(p);
        await db.SaveChangesAsync();
        await Load();
    }

    private async Task Delete(PositionItem item)
    {
        if (item.Employees > 0)
        {
            await DisplayAlert("Видалення неможливе", $"Посаду «{item.Title}» займають {item.Employees} працівн. Спершу змініть їм посаду.", "OK");
            return;
        }
        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Positions.Where(x => x.Id == item.Id).ExecuteDeleteAsync();
        await Load();
    }
}
