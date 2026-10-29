using System.ComponentModel.DataAnnotations;
using LocalBuzz.Models;

namespace LocalBuzz.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(120, ErrorMessage = "Product name cannot exceed 120 characters.")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(60, ErrorMessage = "Brand cannot exceed 60 characters.")]
    [Display(Name = "Brand / Manufacturer")]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [MaxLength(80)]
    [Display(Name = "Category")]
    public string Category { get; set; } = "Electronics";

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description (Features, specs, warranty)")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 10000000.00, ErrorMessage = "Price must be greater than ₹0.")]
    [Display(Name = "Price (₹)")]
    public decimal Price { get; set; }

    [Display(Name = "Available in stock")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Product Image (JPG, PNG, WebP, max 2MB)")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImagePath { get; set; }
}

public class ProductListViewModel
{
    public string ShopName { get; set; } = string.Empty;
    public int ShopId { get; set; }
    public string? SearchQuery { get; set; }
    public string? SelectedCategory { get; set; }
    public List<Product> Products { get; set; } = new();
    public List<string> ShopCategories { get; set; } = new();
}
