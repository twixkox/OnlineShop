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
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using OnlineShopWebApp.Services;


var builder = WebApplication.CreateBuilder(args);

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
    options.AccessDeniedPath = "/Home/Status/403";
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
builder.Services.AddTransient<IReviewStorages, ReviewsDbStorages>();
builder.Services.AddTransient<IPromoCodeStorages, PromoCodesDbStorages>();
builder.Services.AddSingleton<ICookieManager, ChunkingCookieManager>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartOwnerProvider, CartOwnerProvider>();
builder.Services.AddScoped<PromoCodeService>();
builder.Services.AddScoped<OrderNotificationService>();

// Почта: пока не заполнен раздел Smtp, письма пишутся в лог
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

// Онлайн-оплата ЮKassa: включается, когда заданы ShopId и SecretKey
builder.Services.Configure<YooKassaOptions>(builder.Configuration.GetSection(YooKassaOptions.SectionName));
builder.Services.AddHttpClient<IPaymentService, YooKassaPaymentService>();
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

        // Миграции применяются здесь, а не в конструкторах контекстов:
        // иначе база «мигрировала» при каждом создании контекста, а dotnet ef не мог создать новую миграцию
        var identityContext = services.GetRequiredService<IdentityContext>();
        var context = services.GetRequiredService<DatabaseContext>();
        await DatabaseMigrator.MigrateAsync(identityContext);
        await DatabaseMigrator.MigrateAsync(context);

        await IdentityInitializer.InitializeAsync(userManager, roleManager);
        await ShopDbInitilizer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, ex.Message);
    }
}
// Цены выводятся через ToString("C"): без явной культуры на сервере с английской локалью
// вместо рублей показывались доллары
var ruCulture = new CultureInfo("ru-RU");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(ruCulture),
    SupportedCultures = new[] { ruCulture },
    SupportedUICultures = new[] { ruCulture }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
name: "MyArea",
pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
.WithStaticAssets();

// Человекочитаемые адреса: /catalog, /catalog/rozy, /product/{id}
app.MapControllerRoute(
    name: "catalog-all",
    pattern: "catalog",
    defaults: new { controller = "Catalog", action = "AllProducts" });

app.MapControllerRoute(
    name: "catalog-category",
    pattern: "catalog/{slug}",
    defaults: new { controller = "Catalog", action = "Category" },
    // Старые адреса /Catalog/AllProducts и /Catalog/CurrentCategory продолжают работать
    constraints: new { slug = new Microsoft.AspNetCore.Routing.Constraints.RegexRouteConstraint("^(?!(allproducts|currentcategory|category|index)$)[a-z0-9-]+$") });

app.MapControllerRoute(
    name: "product",
    pattern: "product/{id:guid}",
    defaults: new { controller = "Product", action = "Index" });


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
