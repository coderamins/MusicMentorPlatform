using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MusicMentor.Application.DTOs.Admin;
using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.DTOs.Common;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Entities;
using MusicMentor.Domain.Enums;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Services;

public class AdminTeacherService : IAdminTeacherService
{
    private readonly ApplicationDbContext _db;

    public AdminTeacherService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AdminTeacherListItemDto>> SearchAsync(TeacherApprovalStatus? status, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var query = _db.TeacherProfiles.Include(t => t.User).AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.ApprovalStatus == status.Value);

        // PendingReview اول نمایش داده شود تا کار ادمین راحت‌تر پیش برود
        query = query
            .OrderBy(t => t.ApprovalStatus == TeacherApprovalStatus.PendingReview ? 0 : 1)
            .ThenByDescending(t => t.User.CreatedAtUtc);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToListItemDto)
            .ToListAsync();

        return new PagedResult<AdminTeacherListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<AdminTeacherDetailDto?> GetByIdAsync(Guid teacherProfileId)
    {
        return await _db.TeacherProfiles
            .AsNoTracking()
            .Where(t => t.Id == teacherProfileId)
            .Select(ToDetailDto)
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<AdminTeacherDetailDto>> ApproveAsync(Guid teacherProfileId)
    {
        var teacher = await _db.TeacherProfiles.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == teacherProfileId);
        if (teacher is null)
            return ServiceResult<AdminTeacherDetailDto>.Fail("استاد پیدا نشد.");

        teacher.ApprovalStatus = TeacherApprovalStatus.Approved;
        teacher.RejectionReason = null;
        teacher.ReviewedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ServiceResult<AdminTeacherDetailDto>.Success((await GetByIdAsync(teacherProfileId))!);
    }

    public async Task<ServiceResult<AdminTeacherDetailDto>> RejectAsync(Guid teacherProfileId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return ServiceResult<AdminTeacherDetailDto>.Fail("درج دلیل رد الزامی است.");

        var teacher = await _db.TeacherProfiles.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == teacherProfileId);
        if (teacher is null)
            return ServiceResult<AdminTeacherDetailDto>.Fail("استاد پیدا نشد.");

        teacher.ApprovalStatus = TeacherApprovalStatus.Rejected;
        teacher.RejectionReason = reason;
        teacher.ReviewedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ServiceResult<AdminTeacherDetailDto>.Success((await GetByIdAsync(teacherProfileId))!);
    }

    // طبق همون نکته‌ی همیشگی: پروژکشن باید Expression باشه، نه یه متد معمولی که
    // داخل Select صدا زده بشه؛ وگرنه EF Core نمی‌تونه به SQL ترجمه‌ش کنه.
    private static readonly Expression<Func<TeacherProfile, AdminTeacherListItemDto>> ToListItemDto = t => new AdminTeacherListItemDto
    {
        TeacherProfileId = t.Id,
        FullName = t.User.FirstName + " " + t.User.LastName,
        Email = t.User.Email ?? string.Empty,
        PhoneNumber = t.User.PhoneNumber,
        City = t.City,
        District = t.District,
        YearsOfExperience = t.YearsOfExperience,
        HourlyRate = t.HourlyRate,
        ApprovalStatus = t.ApprovalStatus.ToString(),
        RegisteredAtUtc = t.User.CreatedAtUtc,
    };

    private static readonly Expression<Func<TeacherProfile, AdminTeacherDetailDto>> ToDetailDto = t => new AdminTeacherDetailDto
    {
        TeacherProfileId = t.Id,
        FullName = t.User.FirstName + " " + t.User.LastName,
        Email = t.User.Email ?? string.Empty,
        PhoneNumber = t.User.PhoneNumber,
        City = t.City,
        District = t.District,
        YearsOfExperience = t.YearsOfExperience,
        HourlyRate = t.HourlyRate,
        ApprovalStatus = t.ApprovalStatus.ToString(),
        RegisteredAtUtc = t.User.CreatedAtUtc,
        // این‌ها همون منبعی هستن که فعلاً ادمین باهاش تصمیم می‌گیره:
        // توضیح سابقه‌ی کاری/تدریسی که خود استاد موقع ثبت‌نام نوشته + حوزه‌های تدریس
        Bio = t.Bio,
        Categories = t.Categories.Select(c => c.MusicCategory.Name).ToList(),
        RejectionReason = t.RejectionReason,
        ReviewedAtUtc = t.ReviewedAtUtc,
    };
}
