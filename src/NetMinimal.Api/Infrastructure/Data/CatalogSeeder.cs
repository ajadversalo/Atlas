using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetMinimal.Application.Abstractions;
using NetMinimal.Domain.Entities;

namespace NetMinimal.Infrastructure.Data;

public sealed class CatalogSeeder(CatalogDbContext db, IClock clock, ILogger<CatalogSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = clock.UtcNow;
        var hardware = Category.Create("Hardware", "Physical catalog items", now);
        var software = Category.Create("Software", "Licenses and digital goods", now);

        var keyboard = Product.Create("KB-001", "Mechanical Keyboard", "Tactile switches", 129.00m, 25, hardware.Id, now);
        var mouse = Product.Create("MS-014", "Wireless Mouse", "Silent click", 49.50m, 80, hardware.Id, now);
        var license = Product.Create("SW-200", "Pro License", "Annual seat", 299.00m, 500, software.Id, now);

        db.Categories.AddRange(hardware, software);
        db.Products.AddRange(keyboard, mouse, license);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded catalog with {CategoryCount} categories and {ProductCount} products", 2, 3);
    }
}
