namespace MusicMentor.Application.DTOs.Bookings;

/// <summary>بدنه اختیاری برای رد کردن یا لغو یک رزرو (توضیح/دلیل)</summary>
public class BookingActionRequest
{
    public string? Note { get; set; }
}
