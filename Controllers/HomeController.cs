using System.Globalization;
using LocalBuzz.Data;
using LocalBuzz.Services;
using LocalBuzz.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalBuzz.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    // S1: Customer Home (/)
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // S2: Search Results (/Search)
    [HttpGet]
    public async Task<IActionResult> Search(
        string? q,
        string? category,
        string? locality,
        double? lat,
        double? lng,
        double? radiusKm,
        string? sort,
        bool inStock = true,
        int page = 1)
    {
        page = Math.Max(1, page);
        const int pageSize = 12;

        // Base query: products belonging to published shops
        var productsQuery = _context.Products
            .Include(p => p.Shop)
            .Where(p => p.Shop != null && p.Shop.IsPublished);

        // Filter by inStock
        if (inStock)
        {
            productsQuery = productsQuery.Where(p => p.IsAvailable);
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            productsQuery = productsQuery.Where(p => p.Category == category);
        }

        // Locality filter
        if (!string.IsNullOrWhiteSpace(locality) && !locality.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            productsQuery = productsQuery.Where(p => p.Shop!.Locality == locality);
        }

        // Search text matching: every word must match product name, brand, category, or shop name
        var rawTerms = (q ?? string.Empty)
            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLowerInvariant())
            .ToList();

        if (rawTerms.Count > 0)
        {
            foreach (var term in rawTerms)
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(term)) ||
                    p.Category.ToLower().Contains(term) ||
                    p.Shop!.Name.ToLower().Contains(term));
            }
        }

        // Execute query to memory for distance calculation, open hours calculation, and distance sort
        var rawProducts = await productsQuery.ToListAsync();

        // Map to SearchResultItem
        var searchItems = rawProducts.Select(p =>
        {
            var shop = p.Shop!;
            double? distance = null;
            if (lat.HasValue && lng.HasValue && shop.Latitude.HasValue && shop.Longitude.HasValue)
            {
                distance = GeoHelper.HaversineDistanceKm(lat.Value, lng.Value, shop.Latitude.Value, shop.Longitude.Value);
            }

            var isOpen = ShopHours.IsOpenNow(shop.OpeningTime, shop.ClosingTime, shop.WeeklyOff);
            var directions = GeoHelper.GetDirectionsUrl(shop.Latitude, shop.Longitude, shop.Name, shop.Address, shop.Locality, shop.City);

            return new SearchResultItem
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Brand = p.Brand,
                Category = p.Category,
                Description = p.Description,
                Price = p.Price,
                ImagePath = p.ImagePath,
                IsAvailable = p.IsAvailable,
                ShopId = shop.Id,
                ShopName = shop.Name,
                Locality = shop.Locality,
                City = shop.City,
                Address = shop.Address,
                Latitude = shop.Latitude,
                Longitude = shop.Longitude,
                IsOpen = isOpen,
                DistanceKm = distance,
                DirectionsUrl = directions,
                IsLowestPrice = false
            };
        }).ToList();

        // Radius filter if specified and user has coordinates
        if (lat.HasValue && lng.HasValue && radiusKm.HasValue && radiusKm.Value > 0)
        {
            searchItems = searchItems.Where(i => i.DistanceKm.HasValue && i.DistanceKm.Value <= radiusKm.Value).ToList();
        }

        // Identify lowest price among available products
        var availableItems = searchItems.Where(i => i.IsAvailable).ToList();
        if (availableItems.Count > 0)
        {
            var minPrice = availableItems.Min(i => i.Price);
            foreach (var item in availableItems.Where(i => i.Price == minPrice))
            {
                item.IsLowestPrice = true;
            }
        }

        // Sorting
        // Available products always rank before unavailable ones
        var hasCoords = lat.HasValue && lng.HasValue;
        var sortChoice = sort;
        if (string.IsNullOrWhiteSpace(sortChoice))
        {
            sortChoice = hasCoords ? "nearest" : "price_asc";
        }

        IEnumerable<SearchResultItem> sortedList = searchItems;
        if (sortChoice == "nearest" && hasCoords)
        {
            sortedList = searchItems
                .OrderByDescending(i => i.IsAvailable)
                .ThenBy(i => i.DistanceKm ?? double.MaxValue)
                .ThenBy(i => i.Price);
        }
        else if (sortChoice == "price_desc")
        {
            sortedList = searchItems
                .OrderByDescending(i => i.IsAvailable)
                .ThenByDescending(i => i.Price)
                .ThenBy(i => i.DistanceKm ?? double.MaxValue);
        }
        else // default: price_asc
        {
            sortedList = searchItems
                .OrderByDescending(i => i.IsAvailable)
                .ThenBy(i => i.Price)
                .ThenBy(i => i.DistanceKm ?? double.MaxValue);
        }

        var finalList = sortedList.ToList();
        var totalResults = finalList.Count;

        // Paging: 12 per page
        var pagedResults = finalList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Matching shops strip: if search terms exist, check which shops match
        var matchingShops = new List<ShopCardItem>();
        if (rawTerms.Count > 0)
        {
            var shopsQuery = _context.Shops
                .Include(s => s.Products)
                .Where(s => s.IsPublished);

            foreach (var term in rawTerms)
            {
                shopsQuery = shopsQuery.Where(s => s.Name.ToLower().Contains(term) || s.Locality.ToLower().Contains(term));
            }

            var rawMatchingShops = await shopsQuery.Take(4).ToListAsync();
            matchingShops = rawMatchingShops.Select(s =>
            {
                double? distance = null;
                if (lat.HasValue && lng.HasValue && s.Latitude.HasValue && s.Longitude.HasValue)
                {
                    distance = GeoHelper.HaversineDistanceKm(lat.Value, lng.Value, s.Latitude.Value, s.Longitude.Value);
                }

                return new ShopCardItem
                {
                    ShopId = s.Id,
                    Name = s.Name,
                    Locality = s.Locality,
                    City = s.City,
                    Address = s.Address,
                    Phone = s.Phone,
                    HoursFormatted = ShopHours.FormatHours(s.OpeningTime, s.ClosingTime, s.WeeklyOff),
                    IsOpen = ShopHours.IsOpenNow(s.OpeningTime, s.ClosingTime, s.WeeklyOff),
                    DistanceKm = distance,
                    DirectionsUrl = GeoHelper.GetDirectionsUrl(s.Latitude, s.Longitude, s.Name, s.Address, s.Locality, s.City),
                    ProductCount = s.Products.Count(p => p.IsAvailable)
                };
            }).ToList();
        }

        // Distinct localities for dropdown
        var availableLocalities = await _context.Shops
            .Where(s => s.IsPublished)
            .Select(s => s.Locality)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();

        // Build summary line: e.g. "6 shops list 'wireless mouse' · ₹399 to ₹549"
        string? summaryLine = null;
        if (totalResults > 0)
        {
            var distinctShopCount = finalList.Select(i => i.ShopId).Distinct().Count();
            var minP = finalList.Min(i => i.Price);
            var maxP = finalList.Max(i => i.Price);
            var inCulture = new CultureInfo("en-IN");
            var minStr = minP.ToString("C0", inCulture);
            var maxStr = maxP.ToString("C0", inCulture);

            var queryText = !string.IsNullOrWhiteSpace(q) ? $"'{q.Trim()}'" : (!string.IsNullOrWhiteSpace(category) ? $"'{category}'" : "products");
            var shopWord = distinctShopCount == 1 ? "1 shop lists" : $"{distinctShopCount} shops list";
            var priceRange = minP == maxP ? minStr : $"{minStr} to {maxStr}";
            summaryLine = $"{shopWord} {queryText} · {priceRange}";
        }

        var vm = new SearchViewModel
        {
            Q = q,
            Category = category,
            Locality = locality,
            Lat = lat,
            Lng = lng,
            RadiusKm = radiusKm,
            Sort = sortChoice,
            InStock = inStock,
            Page = page,
            PageSize = pageSize,
            TotalResults = totalResults,
            Products = pagedResults,
            MatchingShops = matchingShops,
            AvailableLocalities = availableLocalities,
            SummaryLine = summaryLine
        };

        return View(vm);
    }
}
