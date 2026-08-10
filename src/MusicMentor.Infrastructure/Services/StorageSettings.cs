namespace MusicMentor.Infrastructure.Services;

/// <summary>از appsettings.json (بخش "Storage") پر می‌شود</summary>
public class StorageSettings
{
    /// <summary>
    /// مسیر پایه روی دیسک کانتینر برای ذخیره فایل‌ها. حتماً باید به یک Docker Volume
    /// متصل باشد (در docker-compose.yml) وگرنه با هر بار rebuild کانتینر، فایل‌ها پاک می‌شوند.
    /// </summary>
    public string BasePath { get; set; } = "/app/storage";
}
