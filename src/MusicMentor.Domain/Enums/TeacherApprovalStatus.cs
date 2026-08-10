namespace MusicMentor.Domain.Enums;

/// <summary>وضعیت بررسی/تایید پروفایل استاد توسط ادمین سایت</summary>
public enum TeacherApprovalStatus
{
    /// <summary>استاد ثبت‌نام کرده و منتظر بررسی مدارک/رزومه توسط ادمین است</summary>
    PendingReview = 0,

    /// <summary>ادمین تایید کرده؛ پروفایل استاد در جستجوی عمومی نمایش داده می‌شود و می‌تواند رزرو بپذیرد</summary>
    Approved = 1,

    /// <summary>ادمین رد کرده (دلیل در TeacherProfile.RejectionReason ذخیره می‌شود)</summary>
    Rejected = 2,
}
