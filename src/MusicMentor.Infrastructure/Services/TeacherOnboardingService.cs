using Microsoft.EntityFrameworkCore;
using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.DTOs.Teachers;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Enums;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Services;

public class TeacherOnboardingService : ITeacherOnboardingService
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private const string ResumeSubFolder = "resumes";

    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public TeacherOnboardingService(ApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<ServiceResult<TeacherApprovalStatusDto>> UploadResumeAsync(
        Guid teacherUserId,
        Stream content,
        string originalFileName,
        long fileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        var teacherProfile = await _db.TeacherProfiles.FirstOrDefaultAsync(t => t.UserId == teacherUserId, cancellationToken);
        if (teacherProfile is null)
            return ServiceResult<TeacherApprovalStatusDto>.Fail("پروفایل استاد برای این کاربر پیدا نشد.");

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return ServiceResult<TeacherApprovalStatusDto>.Fail("فرمت فایل مجاز نیست. فقط PDF، DOC یا DOCX قابل قبول است.");

        if (fileSizeBytes <= 0 || fileSizeBytes > MaxFileSizeBytes)
            return ServiceResult<TeacherApprovalStatusDto>.Fail("حجم فایل باید حداکثر ۵ مگابایت باشد.");

        // اگر رزومه قبلی وجود داشت، فایل فیزیکی قدیمی را پاک می‌کنیم تا فضای ذخیره‌سازی هدر نرود
        if (!string.IsNullOrEmpty(teacherProfile.ResumeStoragePath))
            await _fileStorage.DeleteAsync(teacherProfile.ResumeStoragePath, cancellationToken);

        var storageKey = await _fileStorage.SaveAsync(content, originalFileName, ResumeSubFolder, cancellationToken);

        teacherProfile.ResumeStoragePath = storageKey;
        teacherProfile.ResumeFileName = originalFileName;
        teacherProfile.ResumeContentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream",
        };
        teacherProfile.ResumeUploadedAtUtc = DateTime.UtcNow;

        // آپلود رزومه جدید (مثلاً بعد از رد شدن) دوباره پرونده را برای بررسی ادمین باز می‌کند
        if (teacherProfile.ApprovalStatus == TeacherApprovalStatus.Rejected)
        {
            teacherProfile.ApprovalStatus = TeacherApprovalStatus.PendingReview;
            teacherProfile.RejectionReason = null;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<TeacherApprovalStatusDto>.Success(MapToStatusDto(teacherProfile));
    }

    public async Task<ServiceResult<TeacherApprovalStatusDto>> GetMyStatusAsync(Guid teacherUserId)
    {
        var teacherProfile = await _db.TeacherProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == teacherUserId);

        if (teacherProfile is null)
            return ServiceResult<TeacherApprovalStatusDto>.Fail("پروفایل استاد برای این کاربر پیدا نشد.");

        return ServiceResult<TeacherApprovalStatusDto>.Success(MapToStatusDto(teacherProfile));
    }

    private static TeacherApprovalStatusDto MapToStatusDto(Domain.Entities.TeacherProfile t) => new()
    {
        ApprovalStatus = t.ApprovalStatus.ToString(),
        ResumeFileName = t.ResumeFileName,
        ResumeUploadedAtUtc = t.ResumeUploadedAtUtc,
        RejectionReason = t.RejectionReason,
        ReviewedAtUtc = t.ReviewedAtUtc,
    };
}
