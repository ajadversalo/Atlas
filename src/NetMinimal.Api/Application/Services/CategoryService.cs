using Microsoft.Extensions.Logging;
using NetMinimal.Application.Abstractions;
using NetMinimal.Application.Contracts;
using NetMinimal.Application.Mapping;
using NetMinimal.Domain.Entities;
using NetMinimal.Domain.Exceptions;

namespace NetMinimal.Application.Services;

public sealed class CategoryService(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var items = await categories.ListAsync(includeInactive, cancellationToken);
        return items.Select(x => x.ToResponse()).ToList();
    }

    public async Task<CategoryResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);
        return category.ToResponse();
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Name, request.Description, clock.UtcNow);
        await EnsureSlugIsUnique(category.Slug, excludingId: null, cancellationToken);
        await categories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created category {CategoryId} ({Slug})", category.Id, category.Slug);
        return category.ToResponse();
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        category.Update(request.Name, request.Description, request.IsActive, clock.UtcNow);
        await EnsureSlugIsUnique(category.Slug, category.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        if (await categories.HasProductsAsync(id, cancellationToken))
        {
            throw new ConflictException("Cannot delete a category that still has products.");
        }

        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted category {CategoryId}", id);
    }

    private async Task EnsureSlugIsUnique(string slug, Guid? excludingId, CancellationToken cancellationToken)
    {
        var existing = await categories.GetBySlugAsync(slug, cancellationToken);
        if (existing is not null && existing.Id != excludingId)
        {
            throw new ConflictException($"Category slug '{slug}' is already in use.");
        }
    }
}
