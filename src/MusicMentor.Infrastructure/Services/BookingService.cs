using Microsoft.EntityFrameworkCore;
using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.DTOs.Bookings;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Entities;
using MusicMentor.Domain.Enums;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _db;

    public BookingService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<BookingResponseDto>> CreateAsync(Guid studentUserId, CreateBookingRequest request)
    {
        var studentProfile = await _db.StudentProfiles
            .FirstOrDefaultAsync(s => s.UserId == studentUserId);
        if (studentProfile is null)
            return ServiceResult<BookingResponseDto>.Fail("پروفایل هنرآموز برای این کاربر پیدا نشد.");

        var teacherProfile = await _db.TeacherProfiles
            .Include(t => t.Categories)
            .FirstOrDefaultAsync(t => t.Id == request.TeacherProfileId);
        if (teacherProfile is null)
            return ServiceResult<BookingResponseDto>.Fail("استاد مورد نظر پیدا نشد.");

        if (request.SessionStartUtc <= DateTime.UtcNow)
            return ServiceResult<BookingResponseDto>.Fail("زمان شروع کلاس باید در آینده باشد.");

        if (request.DurationMinutes <= 0)
            return ServiceResult<BookingResponseDto>.Fail("مدت‌زمان کلاس نامعتبر است.");

        if (request.MusicCategoryId.HasValue &&
            teacherProfile.Categories.All(c => c.MusicCategoryId != request.MusicCategoryId.Value))
        {
            return ServiceResult<BookingResponseDto>.Fail("این استاد در حوزه انتخاب‌شده تدریس نمی‌کند.");
        }

        var priceAmount = teacherProfile.HourlyRate * request.DurationMinutes / 60m;

        var booking = new Booking
        {
            StudentProfileId = studentProfile.Id,
            TeacherProfileId = teacherProfile.Id,
            MusicCategoryId = request.MusicCategoryId,
            SessionStartUtc = request.SessionStartUtc,
            DurationMinutes = request.DurationMinutes,
            PriceAmount = priceAmount,
            StudentNote = request.StudentNote,
            Status = BookingStatus.PendingTeacherApproval,
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return ServiceResult<BookingResponseDto>.Success(await MapToDtoAsync(booking.Id));
    }

    public async Task<ServiceResult<BookingResponseDto>> ApproveAsync(Guid teacherUserId, Guid bookingId, BookingActionRequest request)
    {
        var booking = await _db.Bookings
            .Include(b => b.TeacherProfile)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
            return ServiceResult<BookingResponseDto>.Fail("رزرو پیدا نشد.");

        if (booking.TeacherProfile.UserId != teacherUserId)
            return ServiceResult<BookingResponseDto>.Fail("شما اجازه دسترسی به این رزرو را ندارید.");

        if (booking.Status != BookingStatus.PendingTeacherApproval)
            return ServiceResult<BookingResponseDto>.Fail("این رزرو در وضعیتی نیست که قابل تایید باشد.");

        booking.Status = BookingStatus.AwaitingPayment;
        booking.TeacherResponseNote = request.Note;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult<BookingResponseDto>.Success(await MapToDtoAsync(booking.Id));
    }

    public async Task<ServiceResult<BookingResponseDto>> RejectAsync(Guid teacherUserId, Guid bookingId, BookingActionRequest request)
    {
        var booking = await _db.Bookings
            .Include(b => b.TeacherProfile)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
            return ServiceResult<BookingResponseDto>.Fail("رزرو پیدا نشد.");

        if (booking.TeacherProfile.UserId != teacherUserId)
            return ServiceResult<BookingResponseDto>.Fail("شما اجازه دسترسی به این رزرو را ندارید.");

        if (booking.Status != BookingStatus.PendingTeacherApproval)
            return ServiceResult<BookingResponseDto>.Fail("این رزرو در وضعیتی نیست که قابل رد کردن باشد.");

        booking.Status = BookingStatus.Rejected;
        booking.TeacherResponseNote = request.Note;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult<BookingResponseDto>.Success(await MapToDtoAsync(booking.Id));
    }

    public async Task<ServiceResult<BookingResponseDto>> CancelAsync(Guid currentUserId, Guid bookingId, BookingActionRequest request)
    {
        var booking = await _db.Bookings
            .Include(b => b.StudentProfile)
            .Include(b => b.TeacherProfile)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
            return ServiceResult<BookingResponseDto>.Fail("رزرو پیدا نشد.");

        var isParticipant = booking.StudentProfile.UserId == currentUserId || booking.TeacherProfile.UserId == currentUserId;
        if (!isParticipant)
            return ServiceResult<BookingResponseDto>.Fail("شما اجازه دسترسی به این رزرو را ندارید.");

        if (booking.Status is not (BookingStatus.PendingTeacherApproval or BookingStatus.AwaitingPayment))
            return ServiceResult<BookingResponseDto>.Fail("این رزرو دیگر قابل لغو نیست.");

        booking.Status = BookingStatus.Cancelled;
        booking.TeacherResponseNote = request.Note ?? booking.TeacherResponseNote;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult<BookingResponseDto>.Success(await MapToDtoAsync(booking.Id));
    }

    public async Task<ServiceResult<BookingResponseDto>> GetByIdAsync(Guid currentUserId, Guid bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.StudentProfile)
            .Include(b => b.TeacherProfile)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
            return ServiceResult<BookingResponseDto>.Fail("رزرو پیدا نشد.");

        var isParticipant = booking.StudentProfile.UserId == currentUserId || booking.TeacherProfile.UserId == currentUserId;
        if (!isParticipant)
            return ServiceResult<BookingResponseDto>.Fail("شما اجازه دسترسی به این رزرو را ندارید.");

        return ServiceResult<BookingResponseDto>.Success(await MapToDtoAsync(booking.Id));
    }

    public async Task<List<BookingResponseDto>> GetMineAsync(Guid currentUserId)
    {
        var studentProfileId = await _db.StudentProfiles
            .Where(s => s.UserId == currentUserId)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync();

        var teacherProfileId = await _db.TeacherProfiles
            .Where(t => t.UserId == currentUserId)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync();

        return await _db.Bookings
            .AsNoTracking()
            .Where(b => b.StudentProfileId == studentProfileId || b.TeacherProfileId == teacherProfileId)
            .OrderByDescending(b => b.CreatedAtUtc)
            .Select(b => ProjectToDto(b))
            .ToListAsync();
    }

    private async Task<BookingResponseDto> MapToDtoAsync(Guid bookingId)
    {
        var dto = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => ProjectToDto(b))
            .FirstAsync();

        return dto;
    }

    private static BookingResponseDto ProjectToDto(Booking b) => new()
    {
        Id = b.Id,
        StudentProfileId = b.StudentProfileId,
        StudentFullName = b.StudentProfile.User.FirstName + " " + b.StudentProfile.User.LastName,
        TeacherProfileId = b.TeacherProfileId,
        TeacherFullName = b.TeacherProfile.User.FirstName + " " + b.TeacherProfile.User.LastName,
        MusicCategoryId = b.MusicCategoryId,
        MusicCategoryName = b.MusicCategory != null ? b.MusicCategory.Name : null,
        SessionStartUtc = b.SessionStartUtc,
        DurationMinutes = b.DurationMinutes,
        PriceAmount = b.PriceAmount,
        Status = b.Status.ToString(),
        StudentNote = b.StudentNote,
        TeacherResponseNote = b.TeacherResponseNote,
        CreatedAtUtc = b.CreatedAtUtc,
        LatestPaymentStatus = b.Payments
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => p.Status.ToString())
            .FirstOrDefault(),
    };
}
