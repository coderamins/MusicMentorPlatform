using Microsoft.AspNetCore.Identity;

namespace MusicMentor.Domain.Entities;

/// <summary>
/// کاربر پایه سیستم. هم استاد و هم هنرآموز از این کلاس مشترک استفاده می‌کنند
/// و تفاوتشان با Role و پروفایل تخصصی (TeacherProfile / StudentProfile) مشخص می‌شود.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    /// <summary>شهر محل سکونت/فعالیت کاربر</summary>
    public string? City { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation properties (هر کاربر حداکثر یکی از این دو پروفایل را دارد)
    public TeacherProfile? TeacherProfile { get; set; }
    public StudentProfile? StudentProfile { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
