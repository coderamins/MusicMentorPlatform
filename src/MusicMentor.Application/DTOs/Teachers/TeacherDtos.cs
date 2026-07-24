namespace MusicMentor.Application.DTOs.Teachers;

/// <summary>خلاصه اطلاعات استاد برای نمایش در لیست/کارت جستجو</summary>
public class TeacherListItemDto
{
    public Guid TeacherProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? District { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal HourlyRate { get; set; }
    public double RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public bool IsVerified { get; set; }
    public string? BioShort { get; set; }
    public List<string> Categories { get; set; } = new();
}

/// <summary>اطلاعات کامل استاد برای صفحه پروفایل</summary>
public class TeacherDetailDto : TeacherListItemDto
{
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public class MusicCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}
