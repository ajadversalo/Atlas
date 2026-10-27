using NetMinimal.Api.Filters;
using NetMinimal.Application.Abstractions;
using NetMinimal.Application.Common;
using NetMinimal.Application.Contracts;
using NetMinimal.Application.Services;

namespace NetMinimal.Api.Endpoints;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/products").WithTags("Products");

        group.MapGet("/", QueryAsync)
            .WithName("QueryProducts")
            .Produces<PagedResult<ProductResponse>>();

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("GetProduct")
            .Produces<ProductResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateProduct")
            .AddEndpointFilter<ValidationFilter<CreateProductRequest>>()
            .Produces<ProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateProduct")
            .AddEndpointFilter<ValidationFilter<UpdateProductRequest>>()
            .Produces<ProductResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/stock", AdjustStockAsync)
            .WithName("AdjustProductStock")
            .AddEndpointFilter<ValidationFilter<AdjustStockRequest>>()
            .Produces<ProductResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteProduct")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return api;
    }

    private static async Task<IResult> QueryAsync(
        IProductService service,
        string? search = null,
        Guid? categoryId = null,
        bool includeInactive = false,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await service.QueryAsync(
            new ProductQuery(search, categoryId, includeInactive, page, pageSize),
            cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        IProductService service,
        CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        return TypedResults.Ok(item);
    }

    private static async Task<IResult> CreateAsync(
        CreateProductRequest request,
        IProductService service,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/api/v1/products/{created.Id}", created);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        IProductService service,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(id, request, cancellationToken);
        return TypedResults.Ok(updated);
    }

    private static async Task<IResult> AdjustStockAsync(
        Guid id,
        AdjustStockRequest request,
        IProductService service,
        CancellationToken cancellationToken)
    {
        var updated = await service.AdjustStockAsync(id, request, cancellationToken);
        return TypedResults.Ok(updated);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IProductService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
}
