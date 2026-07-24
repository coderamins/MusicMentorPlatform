using MusicMentor.Application.DTOs.Common;
using MusicMentor.Application.DTOs.Teachers;

namespace MusicMentor.Application.Interfaces;

public interface ITeacherDirectoryService
{
    Task<PagedResult<TeacherListItemDto>> SearchTeachersAsync(TeacherFilterRequest filter);

    Task<TeacherDetailDto?> GetTeacherByIdAsync(Guid teacherProfileId);

    Task<List<MusicCategoryDto>> GetMusicCategoriesAsync();
}
