using LocalBuzz.Data;
using LocalBuzz.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3309;Database=localbuzz_db;User=root;Password=root123;";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySQL(connectionString);
});

// Cookie Authentication (No ASP.NET Identity)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "LocalBuzz.Auth";
    });

builder.Services.AddAuthorization();

// Custom services
builder.Services.AddScoped<IImageStorage, LocalImageStorage>();

// Controllers with Views & Antiforgery
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "search",
    pattern: "Search",
    defaults: new { controller = "Home", action = "Search" });

app.MapControllerRoute(
    name: "shops_details",
    pattern: "Shops/{id:int}",
    defaults: new { controller = "Shops", action = "Details" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Startup Database connection retry loop & Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();

    const int maxRetries = 15;
    const int delaySeconds = 4;
    var connected = false;

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Testing database connection (Attempt {Attempt}/{MaxRetries})...", attempt, maxRetries);
            if (context.Database.CanConnect())
            {
                connected = true;
                logger.LogInformation("Database is reachable!");
                break;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Database not ready yet (Attempt {Attempt}): {Message}", attempt, ex.Message);
        }

        if (attempt < maxRetries)
        {
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }

    if (connected)
    {
        try
        {
            logger.LogInformation("Applying Database.EnsureCreated()...");
            context.Database.EnsureCreated();

            logger.LogInformation("Running DbSeeder...");
            DbSeeder.SeedAsync(context).GetAwaiter().GetResult();
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while ensuring schema or seeding data.");
        }
    }
    else
    {
        logger.LogError("Could not connect to MySQL database within timeout.");
    }
}

app.Run();
