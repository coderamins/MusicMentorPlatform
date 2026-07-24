namespace MusicMentor.Domain.Enums;

/// <summary>
/// نقش‌های اصلی سیستم. این مقادیر عیناً به عنوان نام Role در ASP.NET Identity
/// هم استفاده می‌شوند (Seed در OnModelCreating / Startup).
/// </summary>
public static class UserRoles
{
    public const string Student = "Student";   // هنرآموز
    public const string Teacher = "Teacher";   // استاد
    public const string Admin = "Admin";       // ادمین پلتفرم

    public static readonly string[] All = { Student, Teacher, Admin };
}
