using Microsoft.EntityFrameworkCore;
using MusicMentor.Application.DTOs.Common;
using MusicMentor.Application.DTOs.Teachers;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Entities;
using MusicMentor.Domain.Enums;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Services;

public class TeacherDirectoryService : ITeacherDirectoryService
{
    private readonly ApplicationDbContext _db;

    public TeacherDirectoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    private class TeacherWithDistance
    {
        public TeacherProfile Teacher { get; set; } = default!;
        public double? DistanceKm { get; set; }
    }

    public async Task<PagedResult<TeacherListItemDto>> SearchTeachersAsync(TeacherFilterRequest filter)
    {
        var baseQuery = _db.TeacherProfiles
            .Include(t => t.User)
            .Include(t => t.Categories)
                .ThenInclude(tc => tc.MusicCategory)
            .Where(t => t.User.IsActive)
            .Where(t => t.ApprovalStatus == TeacherApprovalStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.City))
            baseQuery = baseQuery.Where(t => t.City == filter.City);

        if (!string.IsNullOrWhiteSpace(filter.District))
            baseQuery = baseQuery.Where(t => t.District == filter.District);

        if (filter.MinPrice.HasValue)
            baseQuery = baseQuery.Where(t => t.HourlyRate >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            baseQuery = baseQuery.Where(t => t.HourlyRate <= filter.MaxPrice.Value);

        if (filter.MinExperienceYears.HasValue)
            baseQuery = baseQuery.Where(t => t.YearsOfExperience >= filter.MinExperienceYears.Value);

        if (filter.MusicCategoryIds is { Count: > 0 })
            baseQuery = baseQuery.Where(t => t.Categories.Any(c => filter.MusicCategoryIds.Contains(c.MusicCategoryId)));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            baseQuery = baseQuery.Where(t =>
                EF.Functions.ILike(t.User.FirstName, $"%{term}%") ||
                EF.Functions.ILike(t.User.LastName, $"%{term}%") ||
                (t.Bio != null && EF.Functions.ILike(t.Bio, $"%{term}%")));
        }

        // --- محاسبه فاصله (فقط وقتی هنرآموز موقعیتش رو فرستاده باشه) ---
        // فرمول Haversine عمداً به‌صورت inline نوشته شده (نه یک متد کمکی جدا)،
        // چون EF Core نمی‌تونه فراخوانی متد دلخواه رو به SQL ترجمه کنه - فقط
        // عملیات ریاضی خام (Math.Sin/Cos/Sqrt/Atan2/Pow) قابل ترجمه‌ست.
        IQueryable<TeacherWithDistance> queryWithDistance;

        if (filter.Latitude.HasValue && filter.Longitude.HasValue)
        {
            var lat = filter.Latitude.Value;
            var lng = filter.Longitude.Value;

            queryWithDistance = baseQuery
                .Where(t => t.Latitude != null && t.Longitude != null)
                .Select(t => new TeacherWithDistance
                {
                    Teacher = t,
                    DistanceKm = (double?)(6371.0 * 2.0 * Math.Atan2(
                        Math.Sqrt(
                            Math.Pow(Math.Sin((t.Latitude!.Value - lat) * Math.PI / 180.0 / 2.0), 2) +
                            Math.Cos(lat * Math.PI / 180.0) * Math.Cos(t.Latitude!.Value * Math.PI / 180.0) *
                            Math.Pow(Math.Sin((t.Longitude!.Value - lng) * Math.PI / 180.0 / 2.0), 2)),
                        Math.Sqrt(1.0 -
                            (Math.Pow(Math.Sin((t.Latitude!.Value - lat) * Math.PI / 180.0 / 2.0), 2) +
                            Math.Cos(lat * Math.PI / 180.0) * Math.Cos(t.Latitude!.Value * Math.PI / 180.0) *
                            Math.Pow(Math.Sin((t.Longitude!.Value - lng) * Math.PI / 180.0 / 2.0), 2)))
                    )),
                });

            if (filter.RadiusKm.HasValue)
                queryWithDistance = queryWithDistance.Where(x => x.DistanceKm <= filter.RadiusKm.Value);
        }
        else
        {
            queryWithDistance = baseQuery.Select(t => new TeacherWithDistance { Teacher = t, DistanceKm = null });
        }

        queryWithDistance = filter.SortBy switch
        {
            TeacherSortOption.MostExperienced => queryWithDistance.OrderByDescending(x => x.Teacher.YearsOfExperience),
            TeacherSortOption.PriceLowToHigh => queryWithDistance.OrderBy(x => x.Teacher.HourlyRate),
            TeacherSortOption.PriceHighToLow => queryWithDistance.OrderByDescending(x => x.Teacher.HourlyRate),
            TeacherSortOption.Newest => queryWithDistance.OrderByDescending(x => x.Teacher.User.CreatedAtUtc),
            TeacherSortOption.NearestFirst => queryWithDistance.OrderBy(x => x.DistanceKm),
            _ => queryWithDistance.OrderByDescending(x => x.Teacher.RatingAverage).ThenByDescending(x => x.Teacher.RatingCount),
        };

        var totalCount = await queryWithDistance.CountAsync();

        var page = Math.Max(filter.Page, 1);

        var items = await queryWithDistance
            .Skip((page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new TeacherListItemDto
            {
                TeacherProfileId = x.Teacher.Id,
                UserId = x.Teacher.UserId,
                FullName = x.Teacher.User.FirstName + " " + x.Teacher.User.LastName,
                City = x.Teacher.City,
                District = x.Teacher.District,
                YearsOfExperience = x.Teacher.YearsOfExperience,
                HourlyRate = x.Teacher.HourlyRate,
                RatingAverage = x.Teacher.RatingAverage,
                RatingCount = x.Teacher.RatingCount,
                Latitude=x.Teacher.Latitude,
                Longitude=x.Teacher.Longitude,
                IsVerified = x.Teacher.ApprovalStatus == TeacherApprovalStatus.Approved,
                BioShort = x.Teacher.Bio != null && x.Teacher.Bio.Length > 160
                    ? x.Teacher.Bio.Substring(0, 160) + "…"
                    : x.Teacher.Bio,
                Categories = x.Teacher.Categories.Select(c => c.MusicCategory.Name).ToList(),
                DistanceKm = x.DistanceKm,
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
            .FirstOrDefaultAsync(t =>
                t.Id == teacherProfileId &&
                t.User.IsActive &&
                // همون منطق SearchTeachersAsync: تا وقتی ادمین Approve نکرده،
                // پروفایل عمومی استاد هم نباید با لینک مستقیم قابل مشاهده باشه.
                t.ApprovalStatus == TeacherApprovalStatus.Approved);

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
            // چون کوئری بالا از قبل فیلتر Approved داره، این همیشه true خواهد بود؛
            // شکل DTO رو عمداً عوض نکردم تا فرانت مجبور به تغییر نشه.
            IsVerified = teacher.ApprovalStatus == TeacherApprovalStatus.Approved,
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
