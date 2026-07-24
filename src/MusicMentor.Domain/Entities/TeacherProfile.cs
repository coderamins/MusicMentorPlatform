namespace MusicMentor.Domain.Entities;

/// <summary>
/// اطلاعات تخصصی استاد. با UserId به ApplicationUser وصل می‌شود (رابطه یک‌به‌یک).
/// </summary>
public class TeacherProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;

    /// <summary>بیوگرافی / معرفی استاد</summary>
    public string? Bio { get; set; }

    /// <summary>سابقه تدریس به سال</summary>
    public int YearsOfExperience { get; set; }

    /// <summary>شهریه هر جلسه/ساعت (تومان)</summary>
    public decimal HourlyRate { get; set; }

    public string City { get; set; } = default!;
    public string? District { get; set; } // محله

    /// <summary>وضعیت تایید مدارک/احراز هویت توسط ادمین</summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>میانگین امتیاز از نظرات هنرآموزان (محاسبه‌شده، در فازهای بعد)</summary>
    public double RatingAverage { get; set; } = 0;

    public int RatingCount { get; set; } = 0;

    public ICollection<TeacherMusicCategory> Categories { get; set; } = new List<TeacherMusicCategory>();
}
