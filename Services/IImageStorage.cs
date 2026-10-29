namespace LocalBuzz.Services;

public interface IImageStorage
{
    Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveProductImageAsync(IFormFile? file, CancellationToken cancellationToken = default);
    void DeleteProductImage(string? relativePath);
}
