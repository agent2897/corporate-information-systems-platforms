using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProjectContractsSignalR.Data;
using ProjectContractsSignalR.Hubs;

var culture = new CultureInfo("uk-UA");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR(o => o.EnableDetailedErrors = false);   // клієнт бачить лише HubException
builder.Services.AddDbContext<ContractsContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Contracts") ?? "Data Source=contracts_signalr.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<ContractsContext>().Database.EnsureCreated();

app.UseDefaultFiles();     // wwwroot/index.html — клієнт
app.UseStaticFiles();
app.MapHub<ContractsHub>("/hubs/contracts");
app.Run();
