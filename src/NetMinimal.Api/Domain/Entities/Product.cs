using NetMinimal.Domain.Exceptions;
using NetMinimal.Domain.StaticServices;

namespace NetMinimal.Domain.Entities;

public sealed class Product : Entity
{
    private Product()
    {
    }

    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public static Product Create(
        string sku,
        string name,
        string? description,
        decimal price,
        int stock,
        Guid categoryId,
        DateTimeOffset utcNow)
    {
        var product = new Product { CategoryId = Guard.AgainstEmpty(categoryId) };
        product.Apply(sku, name, description, price, utcNow, isCreate: true);
        product.SetStock(stock, utcNow);
        return product;
    }

    public void Update(
        string name,
        string? description,
        decimal price,
        bool isActive,
        Guid categoryId,
        DateTimeOffset utcNow)
    {
        CategoryId = Guard.AgainstEmpty(categoryId);
        Apply(Sku, name, description, price, utcNow, isCreate: false);
        IsActive = isActive;
    }

    public void AdjustStock(int delta, DateTimeOffset utcNow)
    {
        var next = Stock + delta;
        if (next < 0)
        {
            throw new BusinessRuleException($"Stock for '{Sku}' cannot go below zero.");
        }

        Stock = next;
        UpdatedAtUtc = utcNow;
    }

    public void SetStock(int stock, DateTimeOffset utcNow)
    {
        if (stock < 0)
        {
            throw new BusinessRuleException("Stock cannot be negative.");
        }

        Stock = stock;
        UpdatedAtUtc = utcNow;
    }

    private void Apply(
        string sku,
        string name,
        string? description,
        decimal price,
        DateTimeOffset utcNow,
        bool isCreate)
    {
        Sku = Text.NormalizeSku(Guard.AgainstNullOrWhiteSpace(sku, maxLength: 64));
        Name = Guard.AgainstNullOrWhiteSpace(name, maxLength: 200);
        Description = Guard.Optional(description, maxLength: 4000);

        if (price < 0)
        {
            throw new BusinessRuleException("Price cannot be negative.");
        }

        Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);

        if (isCreate)
        {
            CreatedAtUtc = utcNow;
        }

        UpdatedAtUtc = utcNow;
    }
}
