using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.DTOs.Payments;

namespace MusicMentor.Application.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// برای یک Booking در وضعیت AwaitingPayment، یک تراکنش پرداخت در زرین‌پال ایجاد
    /// و آدرس ریدایرکت به درگاه را برمی‌گرداند.
    /// </summary>
    Task<ServiceResult<CreatePaymentResponse>> RequestPaymentAsync(Guid studentUserId, CreatePaymentRequest request);

    /// <summary>
    /// پس از بازگشت کاربر از درگاه زرین‌پال با Authority/Status صدا زده می‌شود؛
    /// در صورت Status=OK تراکنش را verify و Booking را Confirmed می‌کند.
    /// </summary>
    Task<PaymentCallbackResult> HandleCallbackAsync(string authority, string status);
}
