using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProjectContractsBlazor.Components;
using ProjectContractsBlazor.Data;

var culture = new CultureInfo("uk-UA");
culture.NumberFormat.NumberDecimalSeparator = ".";
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
// У Blazor Server компонент живе довго, тому контекст створюється на кожну операцію через фабрику
builder.Services.AddDbContextFactory<ContractsContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Contracts") ?? "Data Source=contracts_blazor.db"));

var app = builder.Build();

using (var db = app.Services.GetRequiredService<IDbContextFactory<ContractsContext>>().CreateDbContext())
    db.Database.EnsureCreated();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
