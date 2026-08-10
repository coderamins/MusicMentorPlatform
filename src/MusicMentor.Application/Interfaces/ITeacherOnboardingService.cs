using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.DTOs.Teachers;

namespace MusicMentor.Application.Interfaces;

public interface ITeacherOnboardingService
{
    /// <summary>آپلود/جایگزینی فایل رزومه توسط خود استاد</summary>
    Task<ServiceResult<TeacherApprovalStatusDto>> UploadResumeAsync(
        Guid teacherUserId,
        Stream content,
        string originalFileName,
        long fileSizeBytes,
        CancellationToken cancellationToken = default);

    /// <summary>وضعیت فعلی بررسی پروفایل استاد (برای نمایش به خودش، مثلاً «در انتظار تایید ادمین»)</summary>
    Task<ServiceResult<TeacherApprovalStatusDto>> GetMyStatusAsync(Guid teacherUserId);
}
