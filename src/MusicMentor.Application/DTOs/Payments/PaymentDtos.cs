namespace MusicMentor.Application.DTOs.Payments;

public class CreatePaymentRequest
{
    public Guid BookingId { get; set; }
}

public class CreatePaymentResponse
{
    public Guid PaymentId { get; set; }

    /// <summary>آدرسی که کاربر باید برای پرداخت به آن هدایت (Redirect) شود</summary>
    public string PaymentUrl { get; set; } = default!;

    public string Authority { get; set; } = default!;
}

/// <summary>نتیجه‌ی نهایی پس از بازگشت از درگاه و verify شدن تراکنش</summary>
public class PaymentCallbackResult
{
    public bool Success { get; set; }
    public Guid? BookingId { get; set; }
    public string? RefId { get; set; }
    public string? Message { get; set; }
}
