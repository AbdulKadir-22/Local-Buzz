using System.ComponentModel.DataAnnotations;

namespace LocalBuzz.ViewModels;

public class ShopFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Shop name is required.")]
    [MaxLength(100, ErrorMessage = "Shop name cannot exceed 100 characters.")]
    [Display(Name = "Shop Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    [Display(Name = "Address (Street / Building)")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Locality is required.")]
    [MaxLength(80, ErrorMessage = "Locality cannot exceed 80 characters.")]
    [Display(Name = "Locality / Area")]
    public string Locality { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(80, ErrorMessage = "City cannot exceed 80 characters.")]
    [Display(Name = "City")]
    public string City { get; set; } = "Navsari";

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\+?[0-9 ]{8,15}$", ErrorMessage = "Phone must be 8-15 digits with optional '+' and spaces.")]
    [Display(Name = "Phone / WhatsApp")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Opening Time")]
    public TimeOnly OpeningTime { get; set; } = new TimeOnly(9, 0);

    [Required]
    [Display(Name = "Closing Time")]
    public TimeOnly ClosingTime { get; set; } = new TimeOnly(21, 0);

    [Display(Name = "Weekly Off (Optional)")]
    public string? WeeklyOff { get; set; }

    [Display(Name = "Latitude")]
    public double? Latitude { get; set; }

    [Display(Name = "Longitude")]
    public double? Longitude { get; set; }

    [Display(Name = "Publish shop in search results")]
    public bool IsPublished { get; set; } = true;
}

public class DashboardViewModel
{
    public string OwnerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool HasShop => Shop != null;
    public ShopFormViewModel ShopForm { get; set; } = new();
    public LocalBuzz.Models.Shop? Shop { get; set; }

    public int TotalProducts { get; set; }
    public int AvailableProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int CategoriesUsed { get; set; }

    public List<string> ExistingLocalities { get; set; } = new();
}
