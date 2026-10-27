using NetMinimal.Domain.StaticServices;

namespace NetMinimal.Domain.Entities;

public sealed class Category : Entity
{
    private readonly List<Product> _products = [];

    private Category()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public IReadOnlyCollection<Product> Products => _products;

    public static Category Create(string name, string? description, DateTimeOffset utcNow)
    {
        var category = new Category();
        category.Apply(name, description, utcNow, isCreate: true);
        return category;
    }

    public void Update(string name, string? description, bool isActive, DateTimeOffset utcNow)
    {
        Apply(name, description, utcNow, isCreate: false);
        IsActive = isActive;
    }

    private void Apply(string name, string? description, DateTimeOffset utcNow, bool isCreate)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, maxLength: 120);
        Description = Guard.Optional(description, maxLength: 2000);
        Slug = SlugGenerator.From(Name);

        if (isCreate)
        {
            CreatedAtUtc = utcNow;
        }

        UpdatedAtUtc = utcNow;
    }
}
