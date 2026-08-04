using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.DTOs.Bookings;

namespace MusicMentor.Application.Interfaces;

public interface IBookingService
{
    /// <summary>هنرآموز یک درخواست رزرو برای یک استاد ثبت می‌کند</summary>
    Task<ServiceResult<BookingResponseDto>> CreateAsync(Guid studentUserId, CreateBookingRequest request);

    /// <summary>استاد یک درخواست را تایید می‌کند (وضعیت به AwaitingPayment تغییر می‌کند)</summary>
    Task<ServiceResult<BookingResponseDto>> ApproveAsync(Guid teacherUserId, Guid bookingId, BookingActionRequest request);

    /// <summary>استاد یک درخواست را رد می‌کند</summary>
    Task<ServiceResult<BookingResponseDto>> RejectAsync(Guid teacherUserId, Guid bookingId, BookingActionRequest request);

    /// <summary>هنرآموز یا استاد، رزروی که هنوز نهایی (Confirmed) نشده را لغو می‌کند</summary>
    Task<ServiceResult<BookingResponseDto>> CancelAsync(Guid currentUserId, Guid bookingId, BookingActionRequest request);

    /// <summary>یک رزرو با جزئیات؛ فقط اگر currentUserId یکی از طرفین (هنرآموز/استاد) باشد</summary>
    Task<ServiceResult<BookingResponseDto>> GetByIdAsync(Guid currentUserId, Guid bookingId);

    /// <summary>لیست رزروهای کاربر جاری (چه به عنوان هنرآموز، چه به عنوان استاد)</summary>
    Task<List<BookingResponseDto>> GetMineAsync(Guid currentUserId);
}
