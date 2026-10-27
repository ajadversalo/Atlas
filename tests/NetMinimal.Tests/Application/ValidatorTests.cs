using FluentAssertions;
using FluentValidation.TestHelper;
using NetMinimal.Application.Contracts;
using NetMinimal.Application.Validation;

namespace NetMinimal.Tests.Application;

public sealed class ValidatorTests
{
    private readonly CreateProductRequestValidator _createProduct = new();
    private readonly UpdateProductRequestValidator _updateProduct = new();
    private readonly AdjustStockRequestValidator _adjustStock = new();
    private readonly CreateCategoryRequestValidator _createCategory = new();
    private readonly UpdateCategoryRequestValidator _updateCategory = new();

    [Fact]
    public void CreateProduct_accepts_valid_request()
    {
        var result = _createProduct.TestValidate(new CreateProductRequest("SKU", "Name", null, 1m, 0, Guid.NewGuid()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateProduct_rejects_empty_fields_and_negatives()
    {
        var result = _createProduct.TestValidate(new CreateProductRequest("", "", null, -1m, -1, Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.Sku);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Price);
        result.ShouldHaveValidationErrorFor(x => x.Stock);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void UpdateProduct_requires_name_and_category()
    {
        var result = _updateProduct.TestValidate(new UpdateProductRequest("", null, 1m, true, Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void AdjustStock_rejects_zero_delta()
    {
        var result = _adjustStock.TestValidate(new AdjustStockRequest(0));
        result.ShouldHaveValidationErrorFor(x => x.Delta);
    }

    [Fact]
    public void AdjustStock_accepts_nonzero_delta()
    {
        _adjustStock.TestValidate(new AdjustStockRequest(-3)).ShouldNotHaveAnyValidationErrors();
        _adjustStock.TestValidate(new AdjustStockRequest(3)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCategory_requires_name()
    {
        _createCategory.TestValidate(new CreateCategoryRequest("", null)).ShouldHaveValidationErrorFor(x => x.Name);
        _createCategory.TestValidate(new CreateCategoryRequest("Hardware", null)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCategory_requires_name()
    {
        _updateCategory.TestValidate(new UpdateCategoryRequest(" ", null, true)).ShouldHaveValidationErrorFor(x => x.Name);
    }
}
