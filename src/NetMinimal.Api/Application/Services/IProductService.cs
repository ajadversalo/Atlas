using NetMinimal.Application.Abstractions;
using NetMinimal.Application.Common;
using NetMinimal.Application.Contracts;

namespace NetMinimal.Application.Services;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> QueryAsync(ProductQuery query, CancellationToken cancellationToken);
    Task<ProductResponse> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse> AdjustStockAsync(Guid id, AdjustStockRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
