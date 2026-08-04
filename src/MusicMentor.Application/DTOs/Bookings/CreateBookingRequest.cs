namespace MusicMentor.Application.DTOs.Bookings;

/// <summary>درخواست هنرآموز برای رزرو یک جلسه با یک استاد</summary>
public class CreateBookingRequest
{
    public Guid TeacherProfileId { get; set; }

    /// <summary>حوزه/ساز مدنظر (اختیاری - از بین حوزه‌های تدریس همان استاد)</summary>
    public int? MusicCategoryId { get; set; }

    /// <summary>زمان پیشنهادی شروع جلسه (UTC)</summary>
    public DateTime SessionStartUtc { get; set; }

    public int DurationMinutes { get; set; } = 60;

    public string? StudentNote { get; set; }
}
