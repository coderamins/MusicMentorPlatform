using MusicMentor.Application.DTOs.Auth;

namespace MusicMentor.Application.Interfaces;

public interface ITeacherLocationService
{
    Task<ServiceResult<object>> UpdateLocationAsync(Guid teacherUserId, double latitude, double longitude);
}
