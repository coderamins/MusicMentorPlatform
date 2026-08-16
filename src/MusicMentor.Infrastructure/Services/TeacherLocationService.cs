using Microsoft.EntityFrameworkCore;
using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.Interfaces;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Services;

public class TeacherLocationService : ITeacherLocationService
{
    private readonly ApplicationDbContext _db;

    public TeacherLocationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<object>> UpdateLocationAsync(Guid teacherUserId, double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            return ServiceResult<object>.Fail("مقدار latitude نامعتبر است (باید بین ۹۰- تا ۹۰ باشد).");

        if (longitude is < -180 or > 180)
            return ServiceResult<object>.Fail("مقدار longitude نامعتبر است (باید بین ۱۸۰- تا ۱۸۰ باشد).");

        var teacherProfile = await _db.TeacherProfiles.FirstOrDefaultAsync(t => t.UserId == teacherUserId);
        if (teacherProfile is null)
            return ServiceResult<object>.Fail("پروفایل استاد برای این کاربر پیدا نشد.");

        teacherProfile.Latitude = latitude;
        teacherProfile.Longitude = longitude;

        await _db.SaveChangesAsync();

        return ServiceResult<object>.Success(new { latitude, longitude });
    }
}
