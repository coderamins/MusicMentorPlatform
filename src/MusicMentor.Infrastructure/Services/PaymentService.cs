using Microsoft.EntityFrameworkCore;
using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.DTOs.Payments;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Entities;
using MusicMentor.Domain.Enums;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly IZarinPalGateway _gateway;

    public PaymentService(ApplicationDbContext db, IZarinPalGateway gateway)
    {
        _db = db;
        _gateway = gateway;
    }

    public async Task<ServiceResult<CreatePaymentResponse>> RequestPaymentAsync(Guid studentUserId, CreatePaymentRequest request)
    {
        var booking = await _db.Bookings
            .Include(b => b.StudentProfile).ThenInclude(s => s.User)
            .Include(b => b.TeacherProfile).ThenInclude(t => t.User)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking is null)
            return ServiceResult<CreatePaymentResponse>.Fail("رزرو پیدا نشد.");

        if (booking.StudentProfile.UserId != studentUserId)
            return ServiceResult<CreatePaymentResponse>.Fail("شما اجازه پرداخت برای این رزرو را ندارید.");

        if (booking.Status != BookingStatus.AwaitingPayment)
            return ServiceResult<CreatePaymentResponse>.Fail("این رزرو در وضعیتی نیست که قابل پرداخت باشد.");

        var student = booking.StudentProfile.User;
        var description = $"رزرو کلاس با {booking.TeacherProfile.User.FirstName} {booking.TeacherProfile.User.LastName} - {booking.DurationMinutes} دقیقه";

        var gatewayResult = await _gateway.RequestPaymentAsync(
            booking.PriceAmount,
            description,
            mobile: student.PhoneNumber,
            email: student.Email);

        if (!gatewayResult.Success || gatewayResult.Authority is null || gatewayResult.PaymentUrl is null)
            return ServiceResult<CreatePaymentResponse>.Fail(gatewayResult.ErrorMessage ?? "ایجاد تراکنش پرداخت ناموفق بود.");

        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = booking.PriceAmount,
            Gateway = "ZarinPal",
            Authority = gatewayResult.Authority,
            Status = PaymentStatus.Pending,
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return ServiceResult<CreatePaymentResponse>.Success(new CreatePaymentResponse
        {
            PaymentId = payment.Id,
            Authority = gatewayResult.Authority,
            PaymentUrl = gatewayResult.PaymentUrl,
        });
    }

    public async Task<PaymentCallbackResult> HandleCallbackAsync(string authority, string status)
    {
        var payment = await _db.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Authority == authority);

        if (payment is null)
        {
            return new PaymentCallbackResult
            {
                Success = false,
                Message = "تراکنش مورد نظر پیدا نشد.",
            };
        }

        // کاربر در درگاه انصراف داده یا پرداخت ناموفق بوده - رزرو در وضعیت AwaitingPayment باقی می‌ماند تا بتواند دوباره تلاش کند
        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = "پرداخت توسط کاربر لغو شد یا ناموفق بود.";
            await _db.SaveChangesAsync();

            return new PaymentCallbackResult
            {
                Success = false,
                BookingId = payment.BookingId,
                Message = payment.ErrorMessage,
            };
        }

        var verifyResult = await _gateway.VerifyPaymentAsync(payment.Amount, authority);

        if (!verifyResult.Success)
        {
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = verifyResult.ErrorMessage;
            await _db.SaveChangesAsync();

            return new PaymentCallbackResult
            {
                Success = false,
                BookingId = payment.BookingId,
                Message = verifyResult.ErrorMessage ?? "تایید پرداخت ناموفق بود.",
            };
        }

        payment.Status = PaymentStatus.Success;
        payment.RefId = verifyResult.RefId;
        payment.PaidAtUtc = DateTime.UtcNow;

        // این شرط از Confirm شدن دوباره‌ی رزرو در صورت callback تکراری زرین‌پال جلوگیری می‌کند
        if (payment.Booking.Status == BookingStatus.AwaitingPayment)
        {
            payment.Booking.Status = BookingStatus.Confirmed;
            payment.Booking.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return new PaymentCallbackResult
        {
            Success = true,
            BookingId = payment.BookingId,
            RefId = verifyResult.RefId,
            Message = "پرداخت با موفقیت انجام شد.",
        };
    }
}
