namespace LocalBuzz.Services;

public class LocalImageStorage : IImageStorage
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalImageStorage> _logger;
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

    private static readonly Dictionary<string, List<byte[]>> AllowedFileSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".webp", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } } // RIFF header; webp also has WEBP at byte 8
    };

    public LocalImageStorage(IWebHostEnvironment env, ILogger<LocalImageStorage> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveProductImageAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return (true, null, null);
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, null, "Image size exceeds 2 MB limit.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedFileSignatures.ContainsKey(ext))
        {
            return (false, null, "Invalid image format. Allowed formats: .jpg, .jpeg, .png, .webp");
        }

        // Magic bytes inspection
        try
        {
            using var stream = file.OpenReadStream();
            var header = new byte[12];
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);

            if (bytesRead < 4)
            {
                return (false, null, "Invalid or corrupted image file.");
            }

            var signatures = AllowedFileSignatures[ext];
            var match = signatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig));

            if (ext == ".webp")
            {
                // WebP must have RIFF at 0..3 and WEBP at 8..11
                if (bytesRead < 12)
                {
                    match = false;
                }
                else
                {
                    var webpSig = new byte[] { 0x57, 0x45, 0x42, 0x50 };
                    match = match && header.Skip(8).Take(4).SequenceEqual(webpSig);
                }
            }

            if (!match)
            {
                return (false, null, "File content does not match allowed image types (.jpg, .jpeg, .png, .webp).");
            }

            // Save to wwwroot/uploads/products/{Guid}{ext}
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var absolutePath = Path.Combine(uploadsFolder, uniqueFileName);

            stream.Position = 0;
            using (var fileStream = new FileStream(absolutePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream, cancellationToken);
            }

            var relativeUrl = $"/uploads/products/{uniqueFileName}";
            return (true, relativeUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save uploaded image.");
            return (false, null, "An error occurred while saving the image. Please try again.");
        }
    }

    public void DeleteProductImage(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            // Normalize path to prevent traversal
            var trimmed = relativePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, trimmed);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete image file at {Path}", relativePath);
        }
    }
}
