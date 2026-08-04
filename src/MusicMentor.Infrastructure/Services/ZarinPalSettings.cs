namespace MusicMentor.Infrastructure.Services;

/// <summary>از appsettings.json (بخش "ZarinPal") پر می‌شود</summary>
public class ZarinPalSettings
{
    /// <summary>مرچنت‌کد ۳۶ کاراکتری دریافتی از پنل زرین‌پال</summary>
    public string MerchantId { get; set; } = default!;

    /// <summary>
    /// آدرسی که زرین‌پال پس از پرداخت، کاربر را به آن ریدایرکت می‌کند
    /// (باید همان Endpoint کال‌بک ما باشد، مثلاً https://api.musicmentor.ir/api/v1/payments/zarinpal/callback)
    /// </summary>
    public string CallbackUrl { get; set; } = default!;

    /// <summary>در محیط توسعه true؛ در این حالت از درگاه Sandbox زرین‌پال استفاده می‌شود</summary>
    public bool Sandbox { get; set; } = true;

    public string ApiBaseUrl => Sandbox
        ? "https://sandbox.zarinpal.com/pg/v4/payment/"
        : "https://payment.zarinpal.com/pg/v4/payment/";

    public string StartPayBaseUrl => Sandbox
        ? "https://sandbox.zarinpal.com/pg/StartPay/"
        : "https://www.zarinpal.com/pg/StartPay/";
}
