using MusicMentor.Domain.Enums;

namespace MusicMentor.Domain.Entities;

/// <summary>
/// درخواست/رزرو یک جلسه کلاس بین یک هنرآموز و یک استاد.
/// چرخه‌ی وضعیت: PendingTeacherApproval -> AwaitingPayment -> Confirmed
///                                       \-> Rejected
/// در هر مرحله پیش از Confirmed، امکان Cancelled شدن هم هست.
/// </summary>
public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = default!;

    public Guid TeacherProfileId { get; set; }
    public TeacherProfile TeacherProfile { get; set; } = default!;

    /// <summary>حوزه/سازی که این جلسه برایش رزرو شده (اختیاری)</summary>
    public int? MusicCategoryId { get; set; }
    public MusicCategory? MusicCategory { get; set; }

    /// <summary>زمان شروع پیشنهادی جلسه (UTC)</summary>
    public DateTime SessionStartUtc { get; set; }

    public int DurationMinutes { get; set; } = 60;

    /// <summary>
    /// مبلغ نهایی جلسه به تومان؛ در لحظه‌ی ثبت درخواست از روی شهریه‌ی فعلی استاد محاسبه و snapshot می‌شود
    /// تا تغییر بعدی شهریه‌ی استاد روی رزروهای قبلی اثر نگذارد.
    /// </summary>
    public decimal PriceAmount { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.PendingTeacherApproval;

    /// <summary>پیام هنرآموز هنگام ارسال درخواست (اختیاری)</summary>
    public string? StudentNote { get; set; }

    /// <summary>پاسخ/دلیل استاد هنگام تایید یا رد (اختیاری)</summary>
    public string? TeacherResponseNote { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
