namespace MusicMentor.Application.DTOs.Bookings;

public class BookingResponseDto
{
    public Guid Id { get; set; }

    public Guid StudentProfileId { get; set; }
    public string StudentFullName { get; set; } = default!;

    public Guid TeacherProfileId { get; set; }
    public string TeacherFullName { get; set; } = default!;

    public int? MusicCategoryId { get; set; }
    public string? MusicCategoryName { get; set; }

    public DateTime SessionStartUtc { get; set; }
    public int DurationMinutes { get; set; }
    public decimal PriceAmount { get; set; }

    /// <summary>مقدار متنی enum (مثلاً "AwaitingPayment") تا سمت کلاینت بدون وابستگی به عدد enum کار کند</summary>
    public string Status { get; set; } = default!;

    public string? StudentNote { get; set; }
    public string? TeacherResponseNote { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>وضعیت آخرین تلاش پرداخت این رزرو، در صورت وجود</summary>
    public string? LatestPaymentStatus { get; set; }
}
