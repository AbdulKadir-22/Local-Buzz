namespace LocalBuzz.Services;

public record CategoryItem(string Name, string Slug, string Emoji);

public static class CategoryCatalog
{
    public static readonly IReadOnlyList<CategoryItem> Categories = new List<CategoryItem>
    {
        new("Electronics", "electronics", "🖱️"),
        new("Footwear", "footwear", "👟"),
        new("Personal Care", "personal-care", "🧴"),
        new("Groceries", "groceries", "🛒"),
        new("Clothing", "clothing", "👕"),
        new("Stationery", "stationery", "✏️"),
        new("Home & Kitchen", "home-kitchen", "🍳"),
        new("Mobile Accessories", "mobile-accessories", "📱"),
        new("Other", "other", "📦")
    };

    public static string GetEmoji(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName)) return "📦";
        var found = Categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase) ||
                                                  c.Slug.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
        return found?.Emoji ?? "📦";
    }

    public static string GetSlug(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName)) return "other";
        var found = Categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase) ||
                                                  c.Slug.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
        return found?.Slug ?? "other";
    }
}
