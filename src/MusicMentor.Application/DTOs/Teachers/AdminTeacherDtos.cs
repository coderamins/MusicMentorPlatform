namespace MusicMentor.Application.DTOs.Admin;

public class AdminTeacherListItemDto
{
    public Guid TeacherProfileId { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string City { get; set; } = default!;
    public string? District { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal HourlyRate { get; set; }

    /// <summary>PendingReview | Approved | Rejected</summary>
    public string ApprovalStatus { get; set; } = default!;

    public DateTime RegisteredAtUtc { get; set; }
}

/// <summary>
/// جزئیات کامل برای صفحه بررسی ادمین. فعلاً تصمیم تایید/رد صرفاً بر اساس
/// بیوگرافی (سابقه کاری/تدریسی که خود استاد نوشته)، سابقه به سال، و حوزه‌های تدریس گرفته می‌شود.
/// </summary>
public class AdminTeacherDetailDto : AdminTeacherListItemDto
{
    public string? Bio { get; set; }
    public List<string> Categories { get; set; } = new();
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}

public class RejectTeacherRequest
{
    public string Reason { get; set; } = default!;
}
