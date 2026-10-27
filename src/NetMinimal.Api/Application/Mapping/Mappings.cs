using NetMinimal.Application.Contracts;
using NetMinimal.Domain.Entities;

namespace NetMinimal.Application.Mapping;

public static class Mappings
{
    public static CategoryResponse ToResponse(this Category category) =>
        new(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.IsActive,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);

    public static ProductResponse ToResponse(this Product product) =>
        new(
            product.Id,
            product.Sku,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.IsActive,
            product.CategoryId,
            product.Category?.Name,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
}
