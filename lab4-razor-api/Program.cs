using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Data;

var culture = new CultureInfo("uk-UA");
culture.NumberFormat.NumberDecimalSeparator = ".";
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();          // сторінки Razor Pages
builder.Services.AddControllers();         // контролери Web API
builder.Services.AddDbContext<ContractsContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Contracts") ?? "Data Source=contracts_pages.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<ContractsContext>().Database.EnsureCreated();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.Run();
