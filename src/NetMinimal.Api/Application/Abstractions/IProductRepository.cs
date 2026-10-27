using NetMinimal.Application.Common;
using NetMinimal.Domain.Entities;

namespace NetMinimal.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> QueryAsync(ProductQuery query, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Remove(Product product);
}

public sealed record ProductQuery(
    string? Search,
    Guid? CategoryId,
    bool IncludeInactive,
    int Page,
    int PageSize);
