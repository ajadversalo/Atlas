using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetMinimal.Api.Middleware;
using NetMinimal.Application.Abstractions;
using NetMinimal.Application.Services;
using NetMinimal.Infrastructure.Data;
using NetMinimal.Infrastructure.Data.Repositories;
using NetMinimal.Infrastructure.Options;
using NetMinimal.Infrastructure.Services;

namespace NetMinimal.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CatalogConnectionName = "Catalog";

    public static IServiceCollection AddCatalogServices(this IServiceCollection services, IConfiguration configuration)
    {
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

        var connectionString = configuration.GetConnectionString(CatalogConnectionName)
            ?? throw new InvalidOperationException(
                $"Connection string '{CatalogConnectionName}' is missing. Set ConnectionStrings:{CatalogConnectionName}.");

        services.AddDbContext<CatalogDbContext>((sp, options) =>
        {
            var db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            ConfigureProvider(options, db, connectionString);

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

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        if (options.MigrateOnStartup)
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (options.SeedOnStartup)
        {
            var seeder = scope.ServiceProvider.GetRequiredService<CatalogSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
    }

    private static void ConfigureProvider(DbContextOptionsBuilder options, DatabaseOptions db, string connectionString)
    {
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
    }
}
