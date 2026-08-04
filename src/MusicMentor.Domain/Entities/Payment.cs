using MusicMentor.Domain.Enums;

namespace MusicMentor.Domain.Entities;

/// <summary>
/// یک تلاش پرداخت برای یک Booking از طریق درگاه (زرین‌پال).
/// اگر پرداخت اول ناموفق باشد، هنرآموز می‌تواند دوباره تلاش کند؛ به همین دلیل
/// این جدول یک رابطه‌ی چند-به-یک با Booking دارد (چند تلاش برای یک رزرو).
/// </summary>
public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = default!;

    /// <summary>مبلغ ارسالی به درگاه، به تومان</summary>
    public decimal Amount { get; set; }

    public string Gateway { get; set; } = "ZarinPal";

    /// <summary>کد Authority که درگاه هنگام ایجاد تراکنش برمی‌گرداند</summary>
    public string? Authority { get; set; }

    /// <summary>شماره پیگیری که درگاه پس از verify موفق برمی‌گرداند</summary>
    public string? RefId { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAtUtc { get; set; }
}
