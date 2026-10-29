using Microsoft.EntityFrameworkCore;
using LocalBuzz.Models;

namespace LocalBuzz.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Shopkeeper> Shopkeepers => Set<Shopkeeper>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Shopkeeper
        modelBuilder.Entity<Shopkeeper>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.OwnerName).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(120);
            entity.Property(e => e.PasswordHash).IsRequired();

            entity.HasOne(e => e.Shop)
                  .WithOne(s => s.Shopkeeper)
                  .HasForeignKey<Shop>(s => s.ShopkeeperId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Shop
        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Locality);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Locality).IsRequired().HasMaxLength(80);
            entity.Property(e => e.City).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(30);

            entity.Property(e => e.OpeningTime)
                  .HasConversion(
                      t => t.ToTimeSpan(),
                      ts => TimeOnly.FromTimeSpan(ts));

            entity.Property(e => e.ClosingTime)
                  .HasConversion(
                      t => t.ToTimeSpan(),
                      ts => TimeOnly.FromTimeSpan(ts));

            entity.HasMany(e => e.Products)
                  .WithOne(p => p.Shop)
                  .HasForeignKey(p => p.ShopId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
        });
    }
}
