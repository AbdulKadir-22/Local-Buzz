namespace LocalBuzz.ViewModels;

public class SearchResultItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public bool IsAvailable { get; set; }

    // Shop info
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string Locality { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsOpen { get; set; }
    public double? DistanceKm { get; set; }
    public string DirectionsUrl { get; set; } = string.Empty;

    public bool IsLowestPrice { get; set; }
}

public class ShopCardItem
{
    public int ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Locality { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string HoursFormatted { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public double? DistanceKm { get; set; }
    public string DirectionsUrl { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class SearchViewModel
{
    public string? Q { get; set; }
    public string? Category { get; set; }
    public string? Locality { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public double? RadiusKm { get; set; }
    public string? Sort { get; set; }
    public bool InStock { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    public int TotalResults { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public List<SearchResultItem> Products { get; set; } = new();
    public List<ShopCardItem> MatchingShops { get; set; } = new();

    public List<string> AvailableLocalities { get; set; } = new();

    // Summary line
    public string? SummaryLine { get; set; }
}
