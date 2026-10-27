using Microsoft.EntityFrameworkCore;
using NetMinimal.Application.Abstractions;
using NetMinimal.Domain.Entities;

namespace NetMinimal.Infrastructure.Data.Repositories;

public sealed class CategoryRepository(CatalogDbContext db) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        db.Categories.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public async Task<IReadOnlyList<Category>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = db.Categories.AsNoTracking().AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        db.Products.AnyAsync(x => x.CategoryId == categoryId, cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default) =>
        await db.Categories.AddAsync(category, cancellationToken);

    public void Remove(Category category) => db.Categories.Remove(category);
}
