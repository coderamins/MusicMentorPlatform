namespace MusicMentor.Application.Interfaces;

public record ZarinPalRequestResult(bool Success, string? Authority, string? PaymentUrl, string? ErrorMessage);

public record ZarinPalVerifyResult(bool Success, string? RefId, string? ErrorMessage);

/// <summary>انتزاعی از درگاه پرداخت زرین‌پال؛ پیاده‌سازی واقعی (HTTP) در لایه Infrastructure است</summary>
public interface IZarinPalGateway
{
    /// <param name="amountToman">مبلغ به تومان</param>
    Task<ZarinPalRequestResult> RequestPaymentAsync(
        decimal amountToman,
        string description,
        string? mobile,
        string? email,
        CancellationToken cancellationToken = default);

    Task<ZarinPalVerifyResult> VerifyPaymentAsync(
        decimal amountToman,
        string authority,
        CancellationToken cancellationToken = default);
}
