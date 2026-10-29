using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalBuzz.Models;

public class Product
{
    public int Id { get; set; }

    public int ShopId { get; set; }
    public Shop? Shop { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(60)]
    public string? Brand { get; set; }

    [Required]
    [MaxLength(80)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 10000000.00, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [MaxLength(300)]
    public string? ImagePath { get; set; } // e.g. /uploads/products/xyz.jpg

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
