using Microsoft.Extensions.Options;
using MusicMentor.Application.Interfaces;

namespace MusicMentor.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly StorageSettings _settings;

    public LocalFileStorageService(IOptions<StorageSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        var folderFullPath = Path.Combine(_settings.BasePath, subFolder);
        Directory.CreateDirectory(folderFullPath);

        // نام فیزیکی فایل عمداً یک GUID است (نه نام اصلی کاربر) تا از تداخل نام و
        // احتمال Path Traversal جلوگیری شود؛ نام اصلی جدا در دیتابیس (ResumeFileName) نگه‌داری می‌شود.
        var extension = Path.GetExtension(originalFileName);
        var storageKey = $"{subFolder}/{Guid.NewGuid():N}{extension}";

        var fullPath = ResolvePath(storageKey);
        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storageKey;
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(storageKey);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(storageKey);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    /// <summary>storageKey را به مسیر فیزیکی تبدیل می‌کند و مطمئن می‌شود خارج از BasePath نمی‌رود</summary>
    private string ResolvePath(string storageKey)
    {
        var normalized = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_settings.BasePath, normalized));
        var basePathFull = Path.GetFullPath(_settings.BasePath) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(basePathFull, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("مسیر فایل نامعتبر است.");

        return fullPath;
    }
}
