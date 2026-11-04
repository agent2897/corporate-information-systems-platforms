using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectContractsMaui.Data;
using ProjectContractsMaui.Views;

namespace ProjectContractsMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var culture = new CultureInfo("uk-UA");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // База SQLite у приватній папці застосунку на пристрої
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "contracts.db");
        builder.Services.AddDbContextFactory<ContractsContext>(o => o.UseSqlite($"Data Source={dbPath}"));

        // Сторінки створюються контейнером залежностей (у тому числі під час навігації Shell)
        builder.Services.AddTransient<OverviewPage>();
        builder.Services.AddTransient<EmployeesPage>();
        builder.Services.AddTransient<EmployeeEditPage>();
        builder.Services.AddTransient<PositionsPage>();
        builder.Services.AddTransient<ProjectsPage>();
        builder.Services.AddTransient<ProjectEditPage>();
        builder.Services.AddTransient<ProjectDetailsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        var app = builder.Build();

        // Створення схеми й початкових даних (HasData) під час першого запуску
        using (var db = app.Services.GetRequiredService<IDbContextFactory<ContractsContext>>().CreateDbContext())
            db.Database.EnsureCreated();

        return app;
    }
}
