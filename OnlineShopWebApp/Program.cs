using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Db;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShop.Db.Storages;
using OnlineShopWebApp.Areas.Admin.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


// получаем строку подключения из файла конфигурации
string connection = builder.Configuration.GetConnectionString("OnlineShop");

// добавляем контекст DatabaseContext в качестве сервиса в приложение
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connection));

builder.Services.AddDbContext<IdentityContext>(options => options.UseSqlServer(connection));

builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<IdentityContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Authorization/Index";
    options.LogoutPath = "/";
    options.Cookie = new CookieBuilder
    {
        IsEssential = true,
    };
});

builder.Services.AddTransient<IProductStorages, ProductsDbStorages>();
builder.Services.AddTransient<ICartsStorages, CartsDbStorages>();
builder.Services.AddTransient<IOrderStorages, OrdersDbStorages>();
builder.Services.AddTransient<IFavoritesStorages, FavoritesDbStorages>();

// Конфигурация Serilog
 
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(path:"Logs/log.json")
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllersWithViews();
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<User>>();

        await IdentityInitializer.InitializeAsync(userManager, roleManager);  
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, ex.Message);
    }
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();


app.MapControllerRoute(
name: "MyArea",
pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
.WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
