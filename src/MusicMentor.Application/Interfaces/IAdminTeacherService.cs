using MusicMentor.Application.DTOs.Admin;
using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.DTOs.Common;
using MusicMentor.Domain.Enums;

namespace MusicMentor.Application.Interfaces;

public interface IAdminTeacherService
{
    /// <summary>لیست اساتید بر اساس وضعیت بررسی؛ status=null یعنی همه وضعیت‌ها</summary>
    Task<PagedResult<AdminTeacherListItemDto>> SearchAsync(TeacherApprovalStatus? status, int page, int pageSize);

    Task<AdminTeacherDetailDto?> GetByIdAsync(Guid teacherProfileId);

    Task<ServiceResult<AdminTeacherDetailDto>> ApproveAsync(Guid teacherProfileId);

    Task<ServiceResult<AdminTeacherDetailDto>> RejectAsync(Guid teacherProfileId, string reason);

    /// <summary>محتوای فایل رزومه برای دانلود توسط ادمین؛ اگر رزومه‌ای آپلود نشده باشد null برمی‌گرداند</summary>
    Task<(Stream Content, string ContentType, string FileName)?> GetResumeFileAsync(Guid teacherProfileId);
}
