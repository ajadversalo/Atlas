using Microsoft.Extensions.Logging;
using NetMinimal.Application.Abstractions;
using NetMinimal.Application.Common;
using NetMinimal.Application.Contracts;
using NetMinimal.Application.Mapping;
using NetMinimal.Domain.Entities;
using NetMinimal.Domain.Exceptions;
using NetMinimal.Domain.StaticServices;

namespace NetMinimal.Application.Services;

public sealed class ProductService(
    IProductRepository products,
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<ProductService> logger) : IProductService
{
    public async Task<PagedResult<ProductResponse>> QueryAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        var page = Guard.AgainstOutOfRange(query.Page, 1, 10_000);
        var pageSize = Guard.AgainstOutOfRange(query.PageSize, 1, 100);
        var normalized = query with { Page = page, PageSize = pageSize };
        var result = await products.QueryAsync(normalized, cancellationToken);
        return new PagedResult<ProductResponse>(
            result.Items.Select(x => x.ToResponse()).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize);
    }

    public async Task<ProductResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);
        return product.ToResponse();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        await EnsureCategoryExists(request.CategoryId, cancellationToken);
        await EnsureSkuIsUnique(Text.NormalizeSku(request.Sku), excludingId: null, cancellationToken);

        var product = Product.Create(
            request.Sku,
            request.Name,
            request.Description,
            request.Price,
            request.Stock,
            request.CategoryId,
            clock.UtcNow);

        await products.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created product {ProductId} ({Sku})", product.Id, product.Sku);
        return product.ToResponse();
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        await EnsureCategoryExists(request.CategoryId, cancellationToken);
        product.Update(request.Name, request.Description, request.Price, request.IsActive, request.CategoryId, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToResponse();
    }

    public async Task<ProductResponse> AdjustStockAsync(Guid id, AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        product.AdjustStock(request.Delta, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        products.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted product {ProductId}", id);
    }

    private async Task EnsureCategoryExists(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), categoryId);

        if (!category.IsActive)
        {
            throw new BusinessRuleException("Cannot assign a product to an inactive category.");
        }
    }

    private async Task EnsureSkuIsUnique(string sku, Guid? excludingId, CancellationToken cancellationToken)
    {
        var existing = await products.GetBySkuAsync(sku, cancellationToken);
        if (existing is not null && existing.Id != excludingId)
        {
            throw new ConflictException($"SKU '{sku}' is already in use.");
        }
    }
}
