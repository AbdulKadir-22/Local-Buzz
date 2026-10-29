using LocalBuzz.Models;

namespace LocalBuzz.ViewModels;

public class ShopDetailsViewModel
{
    public Shop Shop { get; set; } = null!;
    public bool IsOwner { get; set; }
    public bool IsOpen { get; set; }
    public string HoursFormatted { get; set; } = string.Empty;
    public double? DistanceKm { get; set; }
    public string DirectionsUrl { get; set; } = string.Empty;

    // Filters
    public string? Q { get; set; }
    public string? Category { get; set; }
    public bool InStockOnly { get; set; }

    public List<SearchResultItem> Products { get; set; } = new();
    public List<string> ShopCategories { get; set; } = new();
}
