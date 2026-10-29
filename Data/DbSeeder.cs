using LocalBuzz.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalBuzz.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Idempotency check: only seed if Shopkeepers table is empty
        if (await context.Shopkeepers.AnyAsync())
        {
            return;
        }

        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("shop123");

        // 1. Shopkeepers
        var sk1 = new Shopkeeper
        {
            OwnerName = "Rajesh Mehta",
            Email = "owner1@localbuzz.in",
            PasswordHash = defaultPasswordHash,
            CreatedAt = DateTime.UtcNow
        };

        var sk2 = new Shopkeeper
        {
            OwnerName = "Amit Patel",
            Email = "owner2@localbuzz.in",
            PasswordHash = defaultPasswordHash,
            CreatedAt = DateTime.UtcNow
        };

        var sk3 = new Shopkeeper
        {
            OwnerName = "Pooja Shah",
            Email = "owner3@localbuzz.in",
            PasswordHash = defaultPasswordHash,
            CreatedAt = DateTime.UtcNow
        };

        var sk4 = new Shopkeeper
        {
            OwnerName = "Suresh Desai",
            Email = "owner4@localbuzz.in",
            PasswordHash = defaultPasswordHash,
            CreatedAt = DateTime.UtcNow
        };

        var sk5 = new Shopkeeper
        {
            OwnerName = "Kunal Joshi",
            Email = "newowner@localbuzz.in",
            PasswordHash = defaultPasswordHash,
            CreatedAt = DateTime.UtcNow
        };

        context.Shopkeepers.AddRange(sk1, sk2, sk3, sk4, sk5);
        await context.SaveChangesAsync();

        // 2. Shops (in and around Navsari: center ~ 20.95, 72.95)
        var shop1 = new Shop
        {
            ShopkeeperId = sk1.Id,
            Name = "Navsari Digital Hub",
            Description = "Premium electronics, gaming peripherals, laptop accessories, and computer parts.",
            Address = "14, Station Road, Opp. City Station",
            Locality = "Station Road",
            City = "Navsari",
            Phone = "+91 98251 12345",
            OpeningTime = new TimeOnly(9, 30),
            ClosingTime = new TimeOnly(21, 0),
            WeeklyOff = null,
            Latitude = 20.9525,
            Longitude = 72.9320,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        var shop2 = new Shop
        {
            ShopkeeperId = sk2.Id,
            Name = "Gadget Galaxy",
            Description = "All modern mobile devices, smartwatches, cables, mice, and tech gear at discount rates.",
            Address = "Plot 5, Dudhia Talav Shopping Center",
            Locality = "Dudhia Talav",
            City = "Navsari",
            Phone = "+91 98252 23456",
            OpeningTime = new TimeOnly(10, 0),
            ClosingTime = new TimeOnly(22, 0),
            WeeklyOff = "Sunday",
            Latitude = 20.9480,
            Longitude = 72.9250,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        var shop3 = new Shop
        {
            ShopkeeperId = sk3.Id,
            Name = "Shree Footwear",
            Description = "Branded sneakers, formal leather shoes, casual sandals, running gear for men & women.",
            Address = "G-12, Ambika Arcade, Vijalpore",
            Locality = "Vijalpore",
            City = "Navsari",
            Phone = "+91 98253 34567",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(20, 30),
            WeeklyOff = null,
            Latitude = 20.9380,
            Longitude = 72.9410,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        var shop4 = new Shop
        {
            ShopkeeperId = sk4.Id,
            Name = "Glow Care Store",
            Description = "Organic skin care, dermatological face washes, sunscreens, hair serums, and fragrances.",
            Address = "7, Silver Plaza, Near Lunsikui Circle",
            Locality = "Lunsikui",
            City = "Navsari",
            Phone = "+91 98254 45678",
            OpeningTime = new TimeOnly(10, 0),
            ClosingTime = new TimeOnly(21, 30),
            WeeklyOff = null,
            Latitude = 20.9590,
            Longitude = 72.9180,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Shops.AddRange(shop1, shop2, shop3, shop4);
        await context.SaveChangesAsync();

        // 3. Products (~25 products, same products across shops at different prices, 2-3 unavailable)
        var products = new List<Product>
        {
            // Wireless Mouse comparison (3 shops: ₹399, ₹449, ₹549)
            new Product
            {
                ShopId = shop1.Id,
                Name = "Logitech M170 Wireless Mouse",
                Brand = "Logitech",
                Category = "Electronics",
                Description = "Reliable 2.4GHz wireless connection, 12-month battery life, ambidextrous compact design.",
                Price = 549.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop2.Id,
                Name = "Logitech M170 Wireless Mouse",
                Brand = "Logitech",
                Category = "Electronics",
                Description = "Ergonomic wireless mouse with USB mini receiver and smooth optical tracking.",
                Price = 399.00m, // Lowest price
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop3.Id, // Shree footwear also sells some tech/gadget accessories in this demo
                Name = "Logitech M170 Wireless Mouse",
                Brand = "Logitech",
                Category = "Electronics",
                Description = "Compact optical mouse for laptop and desktop.",
                Price = 449.00m,
                IsAvailable = true
            },

            // USB Keyboard comparison (2 shops)
            new Product
            {
                ShopId = shop1.Id,
                Name = "Dell KB216 Wired USB Keyboard",
                Brand = "Dell",
                Category = "Electronics",
                Description = "Full-sized chiclet multimedia keyboard, spill-resistant and quiet keys.",
                Price = 649.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop2.Id,
                Name = "Dell KB216 Wired USB Keyboard",
                Brand = "Dell",
                Category = "Electronics",
                Description = "Comfortable standard desktop keyboard with durable keycaps.",
                Price = 599.00m, // Lowest price
                IsAvailable = true
            },

            // Running Shoes comparison (2 shops)
            new Product
            {
                ShopId = shop3.Id,
                Name = "Nike Revolution 6 Running Shoes",
                Brand = "Nike",
                Category = "Footwear",
                Description = "Breathable mesh upper with soft foam cushioning for daily road running.",
                Price = 2899.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop2.Id,
                Name = "Nike Revolution 6 Running Shoes",
                Brand = "Nike",
                Category = "Footwear",
                Description = "Lightweight running shoes with high traction rubber sole, size 8-11.",
                Price = 3199.00m,
                IsAvailable = true
            },

            // Face Wash comparison (2 shops)
            new Product
            {
                ShopId = shop4.Id,
                Name = "Cetaphil Gentle Skin Cleanser Face Wash 125ml",
                Brand = "Cetaphil",
                Category = "Personal Care",
                Description = "Hydrating formula for dry to normal sensitive skin with niacinamide and panthenol.",
                Price = 345.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop1.Id,
                Name = "Cetaphil Gentle Skin Cleanser Face Wash 125ml",
                Brand = "Cetaphil",
                Category = "Personal Care",
                Description = "Soap-free, non-comedogenic daily cleanser for clear skin.",
                Price = 370.00m,
                IsAvailable = true
            },

            // Additional Products across shops
            // Shop 1 (Navsari Digital Hub)
            new Product
            {
                ShopId = shop1.Id,
                Name = "SanDisk 64GB Ultra Dual USB Drive Type-C",
                Brand = "SanDisk",
                Category = "Electronics",
                Description = "High-speed USB 3.1 dual flash drive for smartphones, tablets, and computers.",
                Price = 720.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop1.Id,
                Name = "TP-Link Archer C6 Gigabit Wi-Fi Router",
                Brand = "TP-Link",
                Category = "Electronics",
                Description = "AC1200 wireless dual-band router with 4 external antennas and MU-MIMO support.",
                Price = 2399.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop1.Id,
                Name = "Boat Bassheads 242 in-Ear Wired Earphones",
                Brand = "boAt",
                Category = "Mobile Accessories",
                Description = "IPX4 water-resistant sports earphones with mic and 10mm dynamic drivers.",
                Price = 499.00m,
                IsAvailable = false // UNAVAILABLE DEMO 1
            },
            new Product
            {
                ShopId = shop1.Id,
                Name = "HP 150 Wireless Keyboard and Mouse Combo",
                Brand = "HP",
                Category = "Electronics",
                Description = "Plug and play 2.4 GHz wireless combo for productive office workflows.",
                Price = 1299.00m,
                IsAvailable = true
            },

            // Shop 2 (Gadget Galaxy)
            new Product
            {
                ShopId = shop2.Id,
                Name = "Portronics 20W Fast USB-C Power Adapter",
                Brand = "Portronics",
                Category = "Mobile Accessories",
                Description = "Compact PD wall charger compatible with iPhone and Android flagships.",
                Price = 499.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop2.Id,
                Name = "Zebronics 4-Port High Speed USB Hub",
                Brand = "Zebronics",
                Category = "Electronics",
                Description = "Slim USB 3.0 expander with power LED indicator.",
                Price = 249.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop2.Id,
                Name = "OnePlus Nord Buds 2 TWS Earbuds",
                Brand = "OnePlus",
                Category = "Mobile Accessories",
                Description = "True wireless earbuds with Active Noise Cancellation and BassWave enhancement.",
                Price = 2499.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop2.Id,
                Name = "Mi 10000mAh Power Bank 3i",
                Brand = "Xiaomi",
                Category = "Mobile Accessories",
                Description = "Dual output, 18W fast charging portable battery pack.",
                Price = 1199.00m,
                IsAvailable = false // UNAVAILABLE DEMO 2
            },

            // Shop 3 (Shree Footwear)
            new Product
            {
                ShopId = shop3.Id,
                Name = "Puma Smashic Casual Sneakers",
                Brand = "Puma",
                Category = "Footwear",
                Description = "Clean tennis-inspired low-boot leather silhouette with SoftFoam+ comfort insert.",
                Price = 1899.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop3.Id,
                Name = "Bata Formal Derby Leather Shoes",
                Brand = "Bata",
                Category = "Footwear",
                Description = "Classic black formal lace-ups for business, weddings, and formal attire.",
                Price = 1499.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop3.Id,
                Name = "Campus North Running Shoes",
                Brand = "Campus",
                Category = "Footwear",
                Description = "Springy phylon sole everyday walking and workout sneakers.",
                Price = 999.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop3.Id,
                Name = "Sparx Mens Flip Flops & Slides",
                Brand = "Sparx",
                Category = "Footwear",
                Description = "Waterproof cushioned EVA slides for casual beach and daily wear.",
                Price = 379.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop3.Id,
                Name = "Woodland Leather Hiking Boots Khaki",
                Brand = "Woodland",
                Category = "Footwear",
                Description = "Heavy-duty outdoor ankle boots with rugged grip tread.",
                Price = 3995.00m,
                IsAvailable = false // UNAVAILABLE DEMO 3
            },

            // Shop 4 (Glow Care Store)
            new Product
            {
                ShopId = shop4.Id,
                Name = "Derma Co 1% Hyaluronic Sunscreen Aqua Gel SPF 50",
                Brand = "The Derma Co",
                Category = "Personal Care",
                Description = "Broad spectrum PA++++ protection with non-greasy water-light texture.",
                Price = 449.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop4.Id,
                Name = "Minimalist 10% Niacinamide Face Serum 30ml",
                Brand = "Minimalist",
                Category = "Personal Care",
                Description = "Soothing blemish treatment with Zinc and EUK 134 for oil control and texture.",
                Price = 599.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop4.Id,
                Name = "Plum Green Tea Pore Cleansing Face Wash",
                Brand = "Plum",
                Category = "Personal Care",
                Description = "Gentle foaming cleanser with green tea extracts and glycolic acid.",
                Price = 299.00m,
                IsAvailable = true
            },
            new Product
            {
                ShopId = shop4.Id,
                Name = "Mamaearth Onion Hair Oil for Hair Fall Control",
                Brand = "Mamaearth",
                Category = "Personal Care",
                Description = "Redensyl and onion seed oil enriched nourishing scalp therapy.",
                Price = 375.00m,
                IsAvailable = true
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}
