using LocalBuzz.Data;
using LocalBuzz.Services;
using LocalBuzz.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LocalBuzz.Controllers;

public class ShopsController : Controller
{
    private readonly AppDbContext _context;

    public ShopsController(AppDbContext context)
    {
        _context = context;
    }

    // S3: Shop Profile & Catalogue (/Shops/{id})
    [HttpGet]
    public async Task<IActionResult> Details(int id, string? q, string? category, bool inStockOnly, double? lat, double? lng)
    {
        var shop = await _context.Shops
            .Include(s => s.Products)
            .Include(s => s.Shopkeeper)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shop == null)
        {
            return NotFound();
        }

        // Ownership check: is the current user the owner?
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(currentUserIdStr, out var currentUserId);
        var isOwner = (currentUserId > 0 && shop.ShopkeeperId == currentUserId);

        // If not published and not owner, return 404
        if (!shop.IsPublished && !isOwner)
        {
            return NotFound();
        }

        // Distance if coordinates provided
        double? distance = null;
        if (lat.HasValue && lng.HasValue && shop.Latitude.HasValue && shop.Longitude.HasValue)
        {
            distance = GeoHelper.HaversineDistanceKm(lat.Value, lng.Value, shop.Latitude.Value, shop.Longitude.Value);
        }

        var isOpen = ShopHours.IsOpenNow(shop.OpeningTime, shop.ClosingTime, shop.WeeklyOff);
        var directionsUrl = GeoHelper.GetDirectionsUrl(shop.Latitude, shop.Longitude, shop.Name, shop.Address, shop.Locality, shop.City);

        // Product filtering
        var productsQuery = shop.Products.AsQueryable();

        if (inStockOnly)
        {
            productsQuery = productsQuery.Where(p => p.IsAvailable);
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            productsQuery = productsQuery.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLowerInvariant();
            productsQuery = productsQuery.Where(p =>
                p.Name.ToLower().Contains(search) ||
                (p.Brand != null && p.Brand.ToLower().Contains(search)) ||
                p.Category.ToLower().Contains(search));
        }

        var mappedProducts = productsQuery
            .OrderByDescending(p => p.IsAvailable)
            .ThenBy(p => p.Price)
            .Select(p => new SearchResultItem
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
                DirectionsUrl = directionsUrl
            })
            .ToList();

        var shopCategories = shop.Products
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var vm = new ShopDetailsViewModel
        {
            Shop = shop,
            IsOwner = isOwner,
            IsOpen = isOpen,
            HoursFormatted = ShopHours.FormatHours(shop.OpeningTime, shop.ClosingTime, shop.WeeklyOff),
            DistanceKm = distance,
            DirectionsUrl = directionsUrl,
            Q = q,
            Category = category,
            InStockOnly = inStockOnly,
            Products = mappedProducts,
            ShopCategories = shopCategories
        };

        return View(vm);
    }
}
