using System.Security.Claims;
using LocalBuzz.Data;
using LocalBuzz.Models;
using LocalBuzz.Services;
using LocalBuzz.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalBuzz.Controllers;

[Authorize(Roles = "Shopkeeper")]
public class ProductsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IImageStorage _imageStorage;

    public ProductsController(AppDbContext context, IImageStorage imageStorage)
    {
        _context = context;
        _imageStorage = imageStorage;
    }

    private int GetCurrentUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }

    private async Task<Shop?> GetCurrentShopAsync()
    {
        var userId = GetCurrentUserId();
        return await _context.Shops.FirstOrDefaultAsync(s => s.ShopkeeperId == userId);
    }

    // S6: Product List GET (/Products)
    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? category)
    {
        var shop = await GetCurrentShopAsync();
        if (shop == null)
        {
            TempData["ErrorMessage"] = "Please create your shop profile before managing products.";
            return RedirectToAction("Index", "Dashboard");
        }

        var productsQuery = _context.Products
            .Where(p => p.ShopId == shop.Id);

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            productsQuery = productsQuery.Where(p => p.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLowerInvariant();
            productsQuery = productsQuery.Where(p =>
                p.Name.ToLower().Contains(search) ||
                (p.Brand != null && p.Brand.ToLower().Contains(search)));
        }

        var products = await productsQuery
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        var categories = await _context.Products
            .Where(p => p.ShopId == shop.Id)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var vm = new ProductListViewModel
        {
            ShopId = shop.Id,
            ShopName = shop.Name,
            SearchQuery = q,
            SelectedCategory = category,
            Products = products,
            ShopCategories = categories
        };

        return View(vm);
    }

    // S6: Create Product GET (/Products/Create)
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var shop = await GetCurrentShopAsync();
        if (shop == null)
        {
            TempData["ErrorMessage"] = "Please create your shop profile first.";
            return RedirectToAction("Index", "Dashboard");
        }

        var vm = new ProductFormViewModel
        {
            IsAvailable = true,
            Category = "Electronics"
        };

        return View(vm);
    }

    // S6: Create Product POST (/Products/Create)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        var shop = await GetCurrentShopAsync();
        if (shop == null)
        {
            return Forbid();
        }

        // Image validation & upload
        string? savedImagePath = null;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var (success, filePath, errorMessage) = await _imageStorage.SaveProductImageAsync(model.ImageFile);
            if (!success)
            {
                ModelState.AddModelError("ImageFile", errorMessage ?? "Invalid image.");
            }
            else
            {
                savedImagePath = filePath;
            }
        }

        if (!ModelState.IsValid)
        {
            // If image was saved before other validation failed, clean it up
            if (savedImagePath != null)
            {
                _imageStorage.DeleteProductImage(savedImagePath);
            }
            return View(model);
        }

        var product = new Product
        {
            ShopId = shop.Id,
            Name = model.Name.Trim(),
            Brand = model.Brand?.Trim(),
            Category = model.Category.Trim(),
            Description = model.Description?.Trim(),
            Price = model.Price,
            ImagePath = savedImagePath,
            IsAvailable = model.IsAvailable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Product '{product.Name}' added successfully.";
        return RedirectToAction("Index");
    }

    // S6: Edit Product GET (/Products/Edit/{id})
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var shop = await GetCurrentShopAsync();
        if (shop == null)
        {
            return Forbid();
        }

        // Enforce ownership
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shop.Id);
        if (product == null)
        {
            return NotFound();
        }

        var vm = new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Brand = product.Brand,
            Category = product.Category,
            Description = product.Description,
            Price = product.Price,
            IsAvailable = product.IsAvailable,
            ExistingImagePath = product.ImagePath
        };

        return View(vm);
    }

    // S6: Edit Product POST (/Products/Edit/{id})
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        var shop = await GetCurrentShopAsync();
        if (shop == null)
        {
            return Forbid();
        }

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shop.Id);
        if (product == null)
        {
            return NotFound();
        }

        string? newImagePath = null;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var (success, filePath, errorMessage) = await _imageStorage.SaveProductImageAsync(model.ImageFile);
            if (!success)
            {
                ModelState.AddModelError("ImageFile", errorMessage ?? "Invalid image.");
            }
            else
            {
                newImagePath = filePath;
            }
        }

        if (!ModelState.IsValid)
        {
            if (newImagePath != null)
            {
                _imageStorage.DeleteProductImage(newImagePath);
            }
            model.ExistingImagePath = product.ImagePath;
            return View(model);
        }

        // If new image uploaded, delete old image
        if (newImagePath != null)
        {
            if (!string.IsNullOrWhiteSpace(product.ImagePath))
            {
                _imageStorage.DeleteProductImage(product.ImagePath);
            }
            product.ImagePath = newImagePath;
        }

        product.Name = model.Name.Trim();
        product.Brand = model.Brand?.Trim();
        product.Category = model.Category.Trim();
        product.Description = model.Description?.Trim();
        product.Price = model.Price;
        product.IsAvailable = model.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Product '{product.Name}' updated successfully.";
        return RedirectToAction("Index");
    }

    // S6: Toggle Availability POST (/Products/Toggle/{id})
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var shop = await GetCurrentShopAsync();
        if (shop == null)
        {
            return Forbid();
        }

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shop.Id);
        if (product == null)
        {
            return NotFound();
        }

        product.IsAvailable = !product.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = product.IsAvailable
            ? $"'{product.Name}' marked as In Stock."
            : $"'{product.Name}' marked as Out of Stock.";

        return RedirectToAction("Index");
    }

    // S6: Delete Product POST (/Products/Delete/{id})
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var shop = await GetCurrentShopAsync();
        if (shop == null)
        {
            return Forbid();
        }

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shop.Id);
        if (product == null)
        {
            return NotFound();
        }

        // Delete associated image file
        if (!string.IsNullOrWhiteSpace(product.ImagePath))
        {
            _imageStorage.DeleteProductImage(product.ImagePath);
        }

        var productName = product.Name;
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Product '{productName}' was deleted.";
        return RedirectToAction("Index");
    }
}
