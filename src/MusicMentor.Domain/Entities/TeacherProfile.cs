using MusicMentor.Domain.Enums;

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

    /// <summary>
    /// وضعیت بررسی ادمین. تازه بعد از ثبت‌نام همیشه PendingReview است؛ تا وقتی Approved نشود
    /// این استاد در جستجوی عمومی نمایش داده نمی‌شود و نمی‌تواند رزرو بپذیرد.
    /// </summary>
    public TeacherApprovalStatus ApprovalStatus { get; set; } = TeacherApprovalStatus.PendingReview;

    /// <summary>نام اصلی فایل رزومه/مدارک آپلودشده (برای نمایش به کاربر)</summary>
    public string? ResumeFileName { get; set; }

    /// <summary>کلید/مسیر داخلی ذخیره‌سازی فایل رزومه - برای کاربر نهایی نمایش داده نمی‌شود</summary>
    public string? ResumeStoragePath { get; set; }

    public string? ResumeContentType { get; set; }

    public DateTime? ResumeUploadedAtUtc { get; set; }

    /// <summary>در صورت Rejected شدن توسط ادمین، دلیل رد</summary>
    public string? RejectionReason { get; set; }

    /// <summary>زمانی که ادمین آخرین بار Approve/Reject انجام داده</summary>
    public DateTime? ReviewedAtUtc { get; set; }

    /// <summary>میانگین امتیاز از نظرات هنرآموزان (محاسبه‌شده، در فازهای بعد)</summary>
    public double RatingAverage { get; set; } = 0;

    public int RatingCount { get; set; } = 0;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }


    public ICollection<TeacherMusicCategory> Categories { get; set; } = new List<TeacherMusicCategory>();
}
