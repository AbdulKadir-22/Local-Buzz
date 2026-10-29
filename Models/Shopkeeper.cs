using System.ComponentModel.DataAnnotations;

namespace LocalBuzz.Models;

public class Shopkeeper
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string OwnerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Shop? Shop { get; set; }
}
