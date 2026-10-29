using System.ComponentModel.DataAnnotations;

namespace LocalBuzz.Models;

public class Shop
{
    public int Id { get; set; }

    public int ShopkeeperId { get; set; }
    public Shopkeeper? Shopkeeper { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Locality { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string City { get; set; } = "Navsari";

    [Required]
    [RegularExpression(@"^\+?[0-9 ]{8,15}$", ErrorMessage = "Phone must be 8-15 digits with optional '+' and spaces.")]
    public string Phone { get; set; } = string.Empty;

    public TimeOnly OpeningTime { get; set; } = new TimeOnly(9, 0);

    public TimeOnly ClosingTime { get; set; } = new TimeOnly(21, 0);

    [MaxLength(50)]
    public string? WeeklyOff { get; set; } // e.g. "Sunday"

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
