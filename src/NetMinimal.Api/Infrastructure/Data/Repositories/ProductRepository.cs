using Microsoft.EntityFrameworkCore;
using NetMinimal.Application.Abstractions;
using NetMinimal.Application.Common;
using NetMinimal.Domain.Entities;
using NetMinimal.Domain.StaticServices;

namespace NetMinimal.Infrastructure.Data.Repositories;

public sealed class ProductRepository(CatalogDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Products.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
        db.Products.FirstOrDefaultAsync(x => x.Sku == Text.NormalizeSku(sku), cancellationToken);

    public async Task<PagedResult<Product>> QueryAsync(ProductQuery query, CancellationToken cancellationToken = default)
    {
        var items = db.Products.AsNoTracking().Include(x => x.Category).AsQueryable();

        if (!query.IncludeInactive)
        {
            items = items.Where(x => x.IsActive);
        }

        if (query.CategoryId is { } categoryId)
        {
            items = items.Where(x => x.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            items = items.Where(x => x.Name.Contains(term) || x.Sku.Contains(term));
        }

        var total = await items.CountAsync(cancellationToken);
        var pageItems = await items
            .OrderBy(x => x.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>(pageItems, total, query.Page, query.PageSize);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await db.Products.AddAsync(product, cancellationToken);

    public void Remove(Product product) => db.Products.Remove(product);
}
