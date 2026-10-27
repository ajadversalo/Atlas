namespace NetMinimal.Domain.StaticServices;

public static class Text
{
    public static string NormalizeSku(string sku) =>
        Guard.AgainstNullOrWhiteSpace(sku).ToUpperInvariant();
}
