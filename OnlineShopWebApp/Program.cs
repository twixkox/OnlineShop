using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Db;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShop.Db.Storages;
using OnlineShopWebApp.Areas.Admin.Intarfaces;
using OnlineShopWebApp.Areas.Client.Models;
using Serilog;


var builder = WebApplication.CreateBuilder(args);


// получаем строку подключения из файла конфигурации
string connection = builder.Configuration.GetConnectionString("OnlineShop");
#region PathToFiles
var webRootPath = builder.Environment.WebRootPath;
var uploadPaths = new[]
{
    Path.Combine(webRootPath, "uploads", "homePage"),
    Path.Combine(webRootPath, "uploads", "category"),
    Path.Combine(webRootPath, "uploads", "products", "original"),
    Path.Combine(webRootPath, "uploads", "products", "thumbnails"),
    Path.Combine(webRootPath, "uploads", "products", "optimized"),
    Path.Combine(webRootPath, "uploads", "users", "avatars"),
    Path.Combine(webRootPath, "uploads", "temp")
};

foreach (var path  in uploadPaths)
{
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }
}
#endregion


// добавляем контекст DatabaseContext в качестве сервиса в приложение
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connection));

builder.Services.AddDbContext<IdentityContext>(options => options.UseSqlServer(connection));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<IdentityContext>()
.AddDefaultTokenProviders();

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
#region DI-container
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddTransient<IProductStorages, ProductsDbStorages>();
builder.Services.AddTransient<ICartsStorages, CartsDbStorages>();
builder.Services.AddTransient<IOrderStorages, OrdersDbStorages>();
builder.Services.AddTransient<ICategoryStorages, CategoryDbStorages>();
builder.Services.AddTransient<IFavoritesStorages, FavoritesDbStorages>();
builder.Services.AddSingleton<ICookieManager, ChunkingCookieManager>();

#endregion

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles", Serilog.Events.LogEventLevel.Error)
    .WriteTo.Console()
    .WriteTo.File("Logs/log.json")
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
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await ShopDbInitilizer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, ex.Message);
    }
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
name: "MyArea",
pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
.WithStaticAssets();

app.MapControllerRoute(
    name: "catalog",
    pattern: "catalog/{identityUrl}/{categoryId}",
    defaults: new { controller = "Catalog", action = "AllProducts" });


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
