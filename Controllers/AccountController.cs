using System.Security.Claims;
using LocalBuzz.Data;
using LocalBuzz.Models;
using LocalBuzz.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalBuzz.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    // S4: Register GET
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View(new RegisterViewModel());
    }

    // S4: Register POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Shopkeepers
            .AnyAsync(s => s.Email == normalizedEmail);

        if (existingUser)
        {
            ModelState.AddModelError("Email", "An account with this email address already exists.");
            return View(model);
        }

        var shopkeeper = new Shopkeeper
        {
            OwnerName = model.OwnerName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Shopkeepers.Add(shopkeeper);
        await _context.SaveChangesAsync();

        // Sign in automatically
        await SignInShopkeeperAsync(shopkeeper);

        TempData["SuccessMessage"] = $"Welcome to LocalBuzz, {shopkeeper.OwnerName}! Set up your shop profile to get discovered.";
        return RedirectToAction("Index", "Dashboard");
    }

    // S4: Login GET
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // S4: Login POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        var shopkeeper = await _context.Shopkeepers
            .FirstOrDefaultAsync(s => s.Email == normalizedEmail);

        if (shopkeeper == null || !BCrypt.Net.BCrypt.Verify(model.Password, shopkeeper.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await SignInShopkeeperAsync(shopkeeper);

        TempData["SuccessMessage"] = $"Welcome back, {shopkeeper.OwnerName}!";

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    // Logout POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInShopkeeperAsync(Shopkeeper shopkeeper)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, shopkeeper.Id.ToString()),
            new Claim(ClaimTypes.Name, shopkeeper.OwnerName),
            new Claim(ClaimTypes.Email, shopkeeper.Email),
            new Claim(ClaimTypes.Role, "Shopkeeper")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
