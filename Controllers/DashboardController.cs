using System.Security.Claims;
using LocalBuzz.Data;
using LocalBuzz.Models;
using LocalBuzz.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalBuzz.Controllers;

[Authorize(Roles = "Shopkeeper")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }

    // S5: Dashboard Index GET
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var shopkeeper = await _context.Shopkeepers
            .Include(s => s.Shop)
            .ThenInclude(shop => shop!.Products)
            .FirstOrDefaultAsync(s => s.Id == userId);

        if (shopkeeper == null)
        {
            return Forbid();
        }

        var shop = shopkeeper.Shop;

        var existingLocalities = await _context.Shops
            .Select(s => s.Locality)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            OwnerName = shopkeeper.OwnerName,
            Email = shopkeeper.Email,
            Shop = shop,
            ExistingLocalities = existingLocalities
        };

        if (shop != null)
        {
            vm.TotalProducts = shop.Products.Count;
            vm.AvailableProducts = shop.Products.Count(p => p.IsAvailable);
            vm.OutOfStockProducts = shop.Products.Count(p => !p.IsAvailable);
            vm.CategoriesUsed = shop.Products.Select(p => p.Category).Distinct().Count();

            vm.ShopForm = new ShopFormViewModel
            {
                Id = shop.Id,
                Name = shop.Name,
                Description = shop.Description,
                Address = shop.Address,
                Locality = shop.Locality,
                City = shop.City,
                Phone = shop.Phone,
                OpeningTime = shop.OpeningTime,
                ClosingTime = shop.ClosingTime,
                WeeklyOff = shop.WeeklyOff,
                Latitude = shop.Latitude,
                Longitude = shop.Longitude,
                IsPublished = shop.IsPublished
            };
        }
        else
        {
            vm.ShopForm = new ShopFormViewModel
            {
                City = "Navsari",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(21, 0),
                IsPublished = true
            };
        }

        return View(vm);
    }

    // S5: Save Shop Profile POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "ShopForm")] ShopFormViewModel model)
    {
        var userId = GetCurrentUserId();
        var shopkeeper = await _context.Shopkeepers
            .Include(s => s.Shop)
            .FirstOrDefaultAsync(s => s.Id == userId);

        if (shopkeeper == null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            var existingLocalities = await _context.Shops
                .Select(s => s.Locality)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                OwnerName = shopkeeper.OwnerName,
                Email = shopkeeper.Email,
                Shop = shopkeeper.Shop,
                ShopForm = model,
                ExistingLocalities = existingLocalities
            };

            return View("Index", vm);
        }

        if (shopkeeper.Shop == null)
        {
            // Create single shop for this shopkeeper
            var shop = new Shop
            {
                ShopkeeperId = userId,
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                Address = model.Address.Trim(),
                Locality = model.Locality.Trim(),
                City = string.IsNullOrWhiteSpace(model.City) ? "Navsari" : model.City.Trim(),
                Phone = model.Phone.Trim(),
                OpeningTime = model.OpeningTime,
                ClosingTime = model.ClosingTime,
                WeeklyOff = string.IsNullOrWhiteSpace(model.WeeklyOff) ? null : model.WeeklyOff.Trim(),
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                IsPublished = model.IsPublished,
                CreatedAt = DateTime.UtcNow
            };

            _context.Shops.Add(shop);
            TempData["SuccessMessage"] = "Shop profile created successfully! You can now start adding products.";
        }
        else
        {
            // Update existing shop (ownership strictly enforced by userId)
            var shop = shopkeeper.Shop;
            shop.Name = model.Name.Trim();
            shop.Description = model.Description?.Trim();
            shop.Address = model.Address.Trim();
            shop.Locality = model.Locality.Trim();
            shop.City = string.IsNullOrWhiteSpace(model.City) ? "Navsari" : model.City.Trim();
            shop.Phone = model.Phone.Trim();
            shop.OpeningTime = model.OpeningTime;
            shop.ClosingTime = model.ClosingTime;
            shop.WeeklyOff = string.IsNullOrWhiteSpace(model.WeeklyOff) ? null : model.WeeklyOff.Trim();
            shop.Latitude = model.Latitude;
            shop.Longitude = model.Longitude;
            shop.IsPublished = model.IsPublished;

            TempData["SuccessMessage"] = "Shop profile updated successfully.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // Toggle shop publish status
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish()
    {
        var userId = GetCurrentUserId();
        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ShopkeeperId == userId);

        if (shop == null)
        {
            TempData["ErrorMessage"] = "Please create your shop profile first.";
            return RedirectToAction("Index");
        }

        shop.IsPublished = !shop.IsPublished;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = shop.IsPublished
            ? "Your shop is now published and visible to local customers!"
            : "Your shop is now hidden from public search.";

        return RedirectToAction("Index");
    }
}
