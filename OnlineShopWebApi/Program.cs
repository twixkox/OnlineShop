using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using OnlineShop.Db;
using OnlineShop.Db.Interfaces;
using OnlineShop.Db.Models;
using OnlineShop.Db.Storages;
using OnlineShopWebApi;
using OnlineShopWebApi.Middleware;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string connection = builder.Configuration.GetConnectionString("OnlineShop");

        builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // или
        // options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });
        builder.Services.AddAuthorization();

        // добавляем контекст DatabaseContext в качестве сервиса в приложение
        builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connection));

        builder.Services.AddDbContext<IdentityContext>(options => options.UseSqlServer(connection));

        builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<IdentityContext>();

        builder.Services.AddTransient<IProductStorages, ProductsDbStorages>();
        builder.Services.AddTransient<ICartsStorages, CartsDbStorages>();
        builder.Services.AddTransient<IOrderStorages, OrdersDbStorages>();
        builder.Services.AddTransient<IFavoritesStorages, FavoritesDbStorages>();
        builder.Services.AddScoped<IUserService, UserService>();

        Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(path: "Logs/log.json")
    .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "My API",
                Version = "v1",
                Description = "API protected with JWT Authentication"
            });

            option.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });

            option.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });

            builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = false, // Проверяет expiration time
        ClockSkew = TimeSpan.Zero,
            };
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
           
            app.UseSwagger(); 
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnlineShop API V1");
                c.RoutePrefix = string.Empty; 
            });
        }
        

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<JWTMiddleware>();
        app.MapControllers();

        app.Run();
    }
}