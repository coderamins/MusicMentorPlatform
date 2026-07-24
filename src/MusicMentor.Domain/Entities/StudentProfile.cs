namespace MusicMentor.Domain.Entities;

/// <summary>
/// اطلاعات تخصصی هنرآموز (دانش‌آموز). با UserId به ApplicationUser وصل می‌شود.
/// </summary>
public class StudentProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;

    public string City { get; set; } = default!;
    public string? District { get; set; }

    /// <summary>هدف یادگیری / توضیح کوتاه (اختیاری)</summary>
    public string? LearningGoal { get; set; }
}
