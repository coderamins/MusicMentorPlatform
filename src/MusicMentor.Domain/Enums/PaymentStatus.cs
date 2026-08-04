namespace MusicMentor.Domain.Enums;

public enum PaymentStatus
{
    /// <summary>تراکنش در درگاه ایجاد شده، هنوز نتیجه نهایی مشخص نیست</summary>
    Pending = 0,

    /// <summary>پرداخت با موفقیت verify شده</summary>
    Success = 1,

    /// <summary>پرداخت ناموفق بوده یا توسط کاربر لغو شده</summary>
    Failed = 2,
}
