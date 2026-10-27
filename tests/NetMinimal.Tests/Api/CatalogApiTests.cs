using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NetMinimal.Application.Common;
using NetMinimal.Application.Contracts;

namespace NetMinimal.Tests.Api;

public sealed class CatalogApiTests : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client;

    public CatalogApiTests(CatalogApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Seeded_products_are_listed()
    {
        var response = await _client.GetFromJsonAsync<PagedResult<ProductResponse>>("/api/v1/products");
        response.Should().NotBeNull();
        response!.TotalCount.Should().BeGreaterThan(0);
        response.Items.Should().Contain(p => p.Sku == "KB-001");
    }

    [Fact]
    public async Task Seeded_categories_are_listed()
    {
        var categories = await _client.GetFromJsonAsync<List<CategoryResponse>>("/api/v1/categories");
        categories.Should().NotBeNull();
        categories!.Select(c => c.Slug).Should().Contain(["hardware", "software"]);
    }

    [Fact]
    public async Task Create_product_returns_created()
    {
        var categories = await _client.GetFromJsonAsync<List<CategoryResponse>>("/api/v1/categories");
        categories.Should().NotBeNull();
        var categoryId = categories![0].Id;

        var response = await _client.PostAsJsonAsync("/api/v1/products", new CreateProductRequest(
            "CAM-9",
            "USB Camera",
            null,
            79.00m,
            4,
            categoryId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ProductResponse>();
        body!.Sku.Should().Be("CAM-9");
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Adjust_stock_updates_quantity()
    {
        var products = await _client.GetFromJsonAsync<PagedResult<ProductResponse>>("/api/v1/products");
        var keyboard = products!.Items.Single(p => p.Sku == "KB-001");

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/products/{keyboard.Id}/stock",
            new AdjustStockRequest(2));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductResponse>();
        body!.Stock.Should().Be(keyboard.Stock + 2);
    }

    [Fact]
    public async Task Unknown_product_returns_problem_details()
    {
        var response = await _client.GetAsync($"/api/v1/products/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var payload = await response.Content.ReadAsStringAsync();
        payload.Should().Contain("not_found");
    }

    [Fact]
    public async Task Invalid_create_returns_validation_problem()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/products", new CreateProductRequest(
            "",
            "",
            null,
            -1,
            -4,
            Guid.Empty));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Duplicate_sku_returns_conflict()
    {
        var categories = await _client.GetFromJsonAsync<List<CategoryResponse>>("/api/v1/categories");
        var categoryId = categories![0].Id;

        var response = await _client.PostAsJsonAsync("/api/v1/products", new CreateProductRequest(
            "KB-001",
            "Clone",
            null,
            1m,
            1,
            categoryId));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
