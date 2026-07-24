namespace MusicMentor.Application.DTOs.Auth;

public class RegisterTeacherRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string City { get; set; } = default!;
    public string? District { get; set; }

    public string? Bio { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal HourlyRate { get; set; }

    /// <summary>شناسه حوزه‌های تدریس (سازها/سبک‌ها) - از جدول MusicCategory</summary>
    public List<int> MusicCategoryIds { get; set; } = new();
}
