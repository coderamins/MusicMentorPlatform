namespace MusicMentor.Application.DTOs.Teachers;

/// <summary>خروجی وضعیت بررسی پروفایل استاد (برای خود استاد قابل مشاهده است)</summary>
public class TeacherApprovalStatusDto
{
    /// <summary>PendingReview | Approved | Rejected</summary>
    public string ApprovalStatus { get; set; } = default!;

    public string? ResumeFileName { get; set; }
    public DateTime? ResumeUploadedAtUtc { get; set; }

    /// <summary>فقط وقتی ApprovalStatus برابر Rejected باشد پر می‌شود</summary>
    public string? RejectionReason { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }
}
