using MusicMentor.Application.DTOs.Common;
using MusicMentor.Application.DTOs.Students;

namespace MusicMentor.Application.Interfaces;

public interface IStudentDirectoryService
{
    Task<PagedResult<StudentListItemDto>> SearchStudentsAsync(StudentFilterRequest filter);

    Task<StudentDetailDto?> GetStudentByIdAsync(Guid studentProfileId);
}
