using Microsoft.AspNetCore.Mvc;
using MusicMentor.Application.DTOs.Teachers;
using MusicMentor.Application.Interfaces;

namespace MusicMentor.Api.Controllers;

[ApiController]
[Route("api/v1/teachers")]
public class TeachersController : ControllerBase
{
    private readonly ITeacherDirectoryService _teacherDirectoryService;

    public TeachersController(ITeacherDirectoryService teacherDirectoryService)
    {
        _teacherDirectoryService = teacherDirectoryService;
    }

    /// <summary>
    /// جستجو و لیست اساتید با فیلتر شهر/محله/حوزه/شهریه/سابقه و مرتب‌سازی بر اساس محبوبیت، سابقه یا قیمت.
    /// نمونه: /api/v1/teachers?city=تهران&district=ونک&musicCategoryIds=1&musicCategoryIds=6&minPrice=200000&maxPrice=600000&sortBy=MostPopular&page=1&pageSize=12
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] TeacherFilterRequest filter)
    {
        var result = await _teacherDirectoryService.SearchTeachersAsync(filter);
        return Ok(result);
    }

    /// <summary>دریافت پروفایل کامل یک استاد</summary>
    [HttpGet("{teacherProfileId:guid}")]
    [ProducesResponseType(typeof(TeacherDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid teacherProfileId)
    {
        var teacher = await _teacherDirectoryService.GetTeacherByIdAsync(teacherProfileId);
        if (teacher is null)
            return NotFound(new { errors = new[] { "استادی با این شناسه یافت نشد." } });

        return Ok(teacher);
    }
}
