namespace MusicMentor.Domain.Enums;

public enum BookingStatus
{
    /// <summary>هنرآموز درخواست داده، منتظر تایید/رد استاد است</summary>
    PendingTeacherApproval = 0,

    /// <summary>استاد درخواست را رد کرده</summary>
    Rejected = 1,

    /// <summary>استاد تایید کرده، منتظر پرداخت هنرآموز است</summary>
    AwaitingPayment = 2,

    /// <summary>پرداخت موفق انجام شده و کلاس قطعی است</summary>
    Confirmed = 3,

    /// <summary>توسط استاد یا هنرآموز لغو شده (پیش از برگزاری)</summary>
    Cancelled = 4,

    /// <summary>جلسه برگزار شده</summary>
    Completed = 5,
}
