using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetMinimal.Api.Endpoints;
using NetMinimal.Api.Middleware;
using NetMinimal.Application.Abstractions;
using NetMinimal.Application.Services;
using NetMinimal.Infrastructure.Data;
using NetMinimal.Infrastructure.Data.Repositories;
using NetMinimal.Infrastructure.Options;
using NetMinimal.Infrastructure.Services;
using NetMinimal.Infrastructure.StaticServices;
using Scalar.AspNetCore;

const string catalogConnectionName = "Catalog";

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddScoped<IProductService, ProductService>();
services.AddScoped<ICategoryService, CategoryService>();
services.AddValidatorsFromAssembly(typeof(Program).Assembly);

services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOptions<CorsSettings>()
    .Bind(configuration.GetSection(CorsSettings.SectionName))
    .ValidateOnStart();

var connectionString = configuration.GetConnectionString(catalogConnectionName)
    ?? throw new InvalidOperationException(
        $"Connection string '{catalogConnectionName}' is missing. Set ConnectionStrings:{catalogConnectionName}.");

services.AddDbContext<CatalogDbContext>((sp, options) =>
{
    var db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    switch (db.Provider)
    {
        case DatabaseProvider.SqlServer:
            options.UseSqlServer(connectionString, sql =>
            {
                sql.CommandTimeout(db.CommandTimeoutSeconds);
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                sql.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);
            });
            break;

        case DatabaseProvider.Sqlite:
            options.UseSqlite(connectionString, sqlite =>
            {
                sqlite.CommandTimeout(db.CommandTimeoutSeconds);
                sqlite.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);
            });
            break;

        default:
            throw new InvalidOperationException($"Unsupported database provider '{db.Provider}'.");
    }

    if (db.EnableSensitiveDataLogging)
    {
        options.EnableSensitiveDataLogging();
    }

    if (db.EnableDetailedErrors)
    {
        options.EnableDetailedErrors();
    }
});

services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();
services.AddScoped<CatalogSeeder>();
services.AddSingleton<IClock, SystemClock>();
services.AddHealthChecks().AddDbContextCheck<CatalogDbContext>("database");

services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddOpenApi();
services.AddEndpointsApiExplorer();

var cors = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = cors.AllowedOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .ToArray();

        if (origins.Length == 0)
        {
            policy.SetIsOriginAllowed(_ => false);
            return;
        }

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

AppServices.Initialize(app.Services);

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

    if (dbOptions.MigrateOnStartup)
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }

    if (dbOptions.SeedOnStartup)
    {
        var seeder = scope.ServiceProvider.GetRequiredService<CatalogSeeder>();
        await seeder.SeedAsync();
    }
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("NetMinimal Catalog API");
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/scalar"))
    .ExcludeFromDescription();

var api = app.MapGroup("/api/v1").WithOpenApi();
api.MapCategoryEndpoints();
api.MapProductEndpoints();

app.Run();

public partial class Program;
