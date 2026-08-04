using Microsoft.EntityFrameworkCore;
using MusicMentor.Application.DTOs.Common;
using MusicMentor.Application.DTOs.Students;
using MusicMentor.Application.Interfaces;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Services;

public class StudentDirectoryService : IStudentDirectoryService
{
    private const int ShortGoalMaxLength = 160;

    private readonly ApplicationDbContext _db;

    public StudentDirectoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<StudentListItemDto>> SearchStudentsAsync(StudentFilterRequest filter)
    {
        var query = _db.StudentProfiles
            .Include(s => s.User)
            .Where(s => s.User.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(s => s.City == filter.City);

        if (!string.IsNullOrWhiteSpace(filter.District))
            query = query.Where(s => s.District == filter.District);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(s =>
                EF.Functions.ILike(s.User.FirstName, $"%{term}%") ||
                EF.Functions.ILike(s.User.LastName, $"%{term}%"));
        }

        query = filter.SortBy switch
        {
            StudentSortOption.NameAsc => query.OrderBy(s => s.User.FirstName).ThenBy(s => s.User.LastName),
            _ => query.OrderByDescending(s => s.User.CreatedAtUtc),
        };

        var totalCount = await query.CountAsync();

        var page = Math.Max(filter.Page, 1);

        // نکته: پروژکشن باید inline باشد چون EF Core نمی‌تواند فراخوانی متد کمکی
        // دلخواه (مثل یک متد static جدا) را در زمان تولید SQL ترجمه کند.
        var items = await query
            .Skip((page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(s => new StudentListItemDto
            {
                StudentProfileId = s.Id,
                UserId = s.UserId,
                FullName = s.User.FirstName + " " + s.User.LastName,
                City = s.City,
                District = s.District,
                LearningGoalShort = s.LearningGoal != null && s.LearningGoal.Length > ShortGoalMaxLength
                    ? s.LearningGoal.Substring(0, ShortGoalMaxLength) + "…"
                    : s.LearningGoal,
                JoinedAtUtc = s.User.CreatedAtUtc,
            })
            .ToListAsync();

        return new PagedResult<StudentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<StudentDetailDto?> GetStudentByIdAsync(Guid studentProfileId)
    {
        var student = await _db.StudentProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentProfileId && s.User.IsActive);

        if (student is null)
            return null;

        return new StudentDetailDto
        {
            StudentProfileId = student.Id,
            UserId = student.UserId,
            FullName = student.User.FullName,
            City = student.City,
            District = student.District,
            LearningGoalShort = Truncate(student.LearningGoal, ShortGoalMaxLength),
            LearningGoal = student.LearningGoal,
            PhoneNumber = student.User.PhoneNumber,
            Email = student.User.Email,
            JoinedAtUtc = student.User.CreatedAtUtc,
        };
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength] + "…";
    }
}
