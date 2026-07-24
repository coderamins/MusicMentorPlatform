using Microsoft.AspNetCore.Mvc;
using MusicMentor.Application.Interfaces;

namespace MusicMentor.Api.Controllers;

[ApiController]
[Route("api/v1/music-categories")]
public class MusicCategoriesController : ControllerBase
{
    private readonly ITeacherDirectoryService _teacherDirectoryService;

    public MusicCategoriesController(ITeacherDirectoryService teacherDirectoryService)
    {
        _teacherDirectoryService = teacherDirectoryService;
    }

    /// <summary>لیست حوزه‌ها/سازهای قابل تدریس، برای پر کردن فیلتر جستجوی اساتید</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _teacherDirectoryService.GetMusicCategoriesAsync();
        return Ok(categories);
    }
}
