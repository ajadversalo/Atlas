using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetMinimal.Infrastructure.Data;

/// <summary>
/// Used by <c>dotnet ef</c> at design time.
/// </summary>
public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite("Data Source=catalog.db")
            .Options;

        return new CatalogDbContext(options);
    }
}
