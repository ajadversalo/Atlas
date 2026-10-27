using NetMinimal.Api.Filters;
using NetMinimal.Application.Contracts;
using NetMinimal.Application.Services;

namespace NetMinimal.Api.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/categories").WithTags("Categories");

        group.MapGet("/", ListAsync)
            .WithName("ListCategories")
            .Produces<IReadOnlyList<CategoryResponse>>();

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("GetCategory")
            .Produces<CategoryResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateCategory")
            .AddEndpointFilter<ValidationFilter<CreateCategoryRequest>>()
            .Produces<CategoryResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateCategory")
            .AddEndpointFilter<ValidationFilter<UpdateCategoryRequest>>()
            .Produces<CategoryResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteCategory")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return api;
    }

    private static async Task<IResult> ListAsync(
        ICategoryService service,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await service.ListAsync(includeInactive, cancellationToken);
        return TypedResults.Ok(items);
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        ICategoryService service,
        CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return TypedResults.Ok(item);
    }

    private static async Task<IResult> CreateAsync(
        CreateCategoryRequest request,
        ICategoryService service,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/api/v1/categories/{created.Id}", created);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        ICategoryService service,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(id, request, cancellationToken);
        return TypedResults.Ok(updated);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICategoryService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
}
