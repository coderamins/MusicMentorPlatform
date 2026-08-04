using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicMentor.Api.Common;
using MusicMentor.Application.DTOs.Payments;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Enums;

namespace MusicMentor.Api.Controllers;

[ApiController]
[Route("api/v1/payments/zarinpal")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// برای یک رزرو در وضعیت AwaitingPayment، یک تراکنش در زرین‌پال ایجاد می‌کند
    /// و آدرس ریدایرکت به درگاه را برمی‌گرداند. سمت کلاینت باید کاربر را به این آدرس هدایت کند.
    /// </summary>
    [HttpPost("request")]
    [Authorize(Roles = UserRoles.Student)]
    public async Task<IActionResult> RequestPayment([FromBody] CreatePaymentRequest request)
    {
        var result = await _paymentService.RequestPaymentAsync(User.GetUserId(), request);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// آدرس بازگشتی که در Callback زرین‌پال (Jwt__CallbackUrl تنظیم‌شده در appsettings) ثبت می‌شود.
    /// زرین‌پال با query string به شکل ?Authority=...&amp;Status=OK|NOK کاربر را به اینجا ریدایرکت می‌کند.
    /// چون این درخواست از مرورگر کاربر (بدون هدر Authorization) می‌آید، عمداً بدون [Authorize] است.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] string Authority, [FromQuery] string Status)
    {
        var result = await _paymentService.HandleCallbackAsync(Authority, Status);

        // فعلاً یک صفحه‌ی HTML ساده برمی‌گردانیم. وقتی فرانت‌اند آماده شد،
        // به‌جای این، باید کاربر را با Redirect به آدرس نتیجه در فرانت‌اند هدایت کرد، مثلاً:
        // return Redirect($"https://app.musicmentor.ir/payment/result?bookingId={result.BookingId}&success={result.Success}");
        var title = result.Success ? "پرداخت موفق" : "پرداخت ناموفق";
        var color = result.Success ? "#16a34a" : "#dc2626";
        string html = """
            <!DOCTYPE html>
            <html lang="fa" dir="rtl">
            <head>
              <meta charset="utf-8" />
              <title>{title}</title>
              <style>
                body {{ font-family: Tahoma, sans-serif; display: flex; align-items: center; justify-content: center; height: 100vh; margin: 0; background: #f4f4f5; }}
                .card {{ background: #fff; padding: 32px 40px; border-radius: 12px; box-shadow: 0 2px 12px rgba(0,0,0,.08); text-align: center; max-width: 360px; }}
                h1 {{ color: {color}; font-size: 20px; margin-bottom: 8px; }}
                p {{ color: #555; font-size: 14px; }}
              </style>
            </head>        
            <body>
              <div class="card">
                <h1>{title}</h1>
                <p>{result.Message}</p>
              </div>
            </body>
            </html>
            """;

        return Content(html, "text/html; charset=utf-8");
    }
}
