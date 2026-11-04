using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProjectContractsMvc.Data;

var culture = new CultureInfo("uk-UA");
culture.NumberFormat.NumberDecimalSeparator = ".";   // щоб оклад "52000.50" коректно приймався з форми
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ContractsContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Contracts") ?? "Data Source=contracts_mvc.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<ContractsContext>().Database.EnsureCreated();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
