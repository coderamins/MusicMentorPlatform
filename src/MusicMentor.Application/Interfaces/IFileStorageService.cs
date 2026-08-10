namespace MusicMentor.Application.Interfaces;

/// <summary>
/// انتزاعی برای ذخیره فایل (فعلاً فقط رزومه استاد). پیاده‌سازی پیش‌فرض روی دیسک محلی است
/// (LocalFileStorageService در Infrastructure)؛ در آینده می‌توان بدون تغییر در سرویس‌های
/// بالادستی، پیاده‌سازی S3/Object Storage جایگزین کرد.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// محتوای stream را ذخیره می‌کند و یک "storageKey" داخلی برمی‌گرداند
    /// (برای خواندن/حذف بعدی از همین سرویس استفاده می‌شود - نباید مستقیماً به کاربر نشان داده شود).
    /// </summary>
    Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string subFolder,
        CancellationToken cancellationToken = default);

    /// <summary>در صورت وجود فایل، یک Stream برای خواندن برمی‌گرداند؛ در غیر این صورت null.</summary>
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
