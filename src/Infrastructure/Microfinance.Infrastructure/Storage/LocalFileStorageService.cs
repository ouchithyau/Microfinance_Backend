using Microfinance.Application.Common.Interfaces;

namespace Microfinance.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;

    public LocalFileStorageService()
    {
        _baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderName, CancellationToken cancellationToken = default)
    {
        var folderPath = Path.Combine(_baseStoragePath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        using var outputStream = new FileStream(fullPath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream, cancellationToken);

        return Path.Combine("Uploads", folderName, uniqueFileName).Replace("\\", "/");
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public async Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Requested file was not found.", filePath);
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }
}