namespace MusicMentor.Application.DTOs.Students;

/// <summary>خلاصه اطلاعات هنرجو برای نمایش در لیست</summary>
public class StudentListItemDto
{
    public Guid StudentProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? District { get; set; }
    public string? LearningGoalShort { get; set; }
    public DateTime JoinedAtUtc { get; set; }
}

/// <summary>اطلاعات کامل هنرجو برای صفحه پروفایل (فقط برای Teacher/Admin قابل مشاهده)</summary>
public class StudentDetailDto : StudentListItemDto
{
    public string? LearningGoal { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
