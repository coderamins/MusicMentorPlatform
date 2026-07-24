using MusicMentor.Application.DTOs.Auth;

namespace MusicMentor.Application.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterStudentAsync(RegisterStudentRequest request);
    Task<ServiceResult<AuthResponse>> RegisterTeacherAsync(RegisterTeacherRequest request);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
}
