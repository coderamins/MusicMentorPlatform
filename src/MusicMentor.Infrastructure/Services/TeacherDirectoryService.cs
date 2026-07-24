using Microsoft.EntityFrameworkCore;
using MusicMentor.Application.DTOs.Common;
using MusicMentor.Application.DTOs.Teachers;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Entities;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Services;

public class TeacherDirectoryService : ITeacherDirectoryService
{
    private readonly ApplicationDbContext _db;

    public TeacherDirectoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<TeacherListItemDto>> SearchTeachersAsync(TeacherFilterRequest filter)
    {
        var query = _db.TeacherProfiles
            .Include(t => t.User)
            .Include(t => t.Categories)
                .ThenInclude(tc => tc.MusicCategory)
            .Where(t => t.User.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(t => t.City == filter.City);

        if (!string.IsNullOrWhiteSpace(filter.District))
            query = query.Where(t => t.District == filter.District);

        if (filter.MinPrice.HasValue)
            query = query.Where(t => t.HourlyRate >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(t => t.HourlyRate <= filter.MaxPrice.Value);

        if (filter.MinExperienceYears.HasValue)
            query = query.Where(t => t.YearsOfExperience >= filter.MinExperienceYears.Value);

        if (filter.OnlyVerified == true)
            query = query.Where(t => t.IsVerified);

        if (filter.MusicCategoryIds is { Count: > 0 })
            query = query.Where(t => t.Categories.Any(c => filter.MusicCategoryIds.Contains(c.MusicCategoryId)));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(t =>
                EF.Functions.ILike(t.User.FirstName, $"%{term}%") ||
                EF.Functions.ILike(t.User.LastName, $"%{term}%") ||
                (t.Bio != null && EF.Functions.ILike(t.Bio, $"%{term}%")));
        }

        query = filter.SortBy switch
        {
            TeacherSortOption.MostExperienced => query.OrderByDescending(t => t.YearsOfExperience),
            TeacherSortOption.PriceLowToHigh => query.OrderBy(t => t.HourlyRate),
            TeacherSortOption.PriceHighToLow => query.OrderByDescending(t => t.HourlyRate),
            TeacherSortOption.Newest => query.OrderByDescending(t => t.User.CreatedAtUtc),
            _ => query.OrderByDescending(t => t.RatingAverage).ThenByDescending(t => t.RatingCount),
        };

        var totalCount = await query.CountAsync();

        var page = Math.Max(filter.Page, 1);

        // نکته: پروژکشن باید inline باشد چون EF Core نمی‌تواند فراخوانی متد کمکی
        // دلخواه (مثل یک متد static جدا) را در زمان تولید SQL ترجمه کند.
        var items = await query
            .Skip((page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new TeacherListItemDto
            {
                TeacherProfileId = t.Id,
                UserId = t.UserId,
                FullName = t.User.FirstName + " " + t.User.LastName,
                City = t.City,
                District = t.District,
                YearsOfExperience = t.YearsOfExperience,
                HourlyRate = t.HourlyRate,
                RatingAverage = t.RatingAverage,
                RatingCount = t.RatingCount,
                IsVerified = t.IsVerified,
                BioShort = t.Bio != null && t.Bio.Length > 160 ? t.Bio.Substring(0, 160) + "…" : t.Bio,
                Categories = t.Categories.Select(c => c.MusicCategory.Name).ToList(),
            })
            .ToListAsync();

        return new PagedResult<TeacherListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<TeacherDetailDto?> GetTeacherByIdAsync(Guid teacherProfileId)
    {
        var teacher = await _db.TeacherProfiles
            .Include(t => t.User)
            .Include(t => t.Categories)
                .ThenInclude(tc => tc.MusicCategory)
            .FirstOrDefaultAsync(t => t.Id == teacherProfileId && t.User.IsActive);

        if (teacher is null)
            return null;

        return new TeacherDetailDto
        {
            TeacherProfileId = teacher.Id,
            UserId = teacher.UserId,
            FullName = teacher.User.FullName,
            City = teacher.City,
            District = teacher.District,
            YearsOfExperience = teacher.YearsOfExperience,
            HourlyRate = teacher.HourlyRate,
            RatingAverage = teacher.RatingAverage,
            RatingCount = teacher.RatingCount,
            IsVerified = teacher.IsVerified,
            BioShort = Truncate(teacher.Bio, 160),
            Bio = teacher.Bio,
            PhoneNumber = teacher.User.PhoneNumber,
            Email = teacher.User.Email,
            Categories = teacher.Categories.Select(c => c.MusicCategory.Name).ToList(),
        };
    }

    public async Task<List<MusicCategoryDto>> GetMusicCategoriesAsync()
    {
        return await _db.MusicCategories
            .OrderBy(c => c.Name)
            .Select(c => new MusicCategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength] + "…";
    }
}
