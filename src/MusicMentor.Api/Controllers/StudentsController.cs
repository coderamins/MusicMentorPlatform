using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicMentor.Application.DTOs.Students;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Enums;

namespace MusicMentor.Api.Controllers;

[ApiController]
[Route("api/v1/students")]
[Authorize(Roles = $"{UserRoles.Teacher},{UserRoles.Admin}")]
public class StudentsController : ControllerBase
{
    private readonly IStudentDirectoryService _studentDirectoryService;

    public StudentsController(IStudentDirectoryService studentDirectoryService)
    {
        _studentDirectoryService = studentDirectoryService;
    }

    /// <summary>
    /// جستجو و لیست هنرجوها با فیلتر شهر/محله و نام. فقط برای اساتید و ادمین در دسترس است
    /// (برخلاف لیست اساتید که عمومی است) چون شامل اطلاعات شخصی هنرجوهاست.
    /// نمونه: /api/v1/students?city=تهران&search=مینا&sortBy=Newest&page=1&pageSize=12
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] StudentFilterRequest filter)
    {
        var result = await _studentDirectoryService.SearchStudentsAsync(filter);
        return Ok(result);
    }

    /// <summary>دریافت پروفایل کامل یک هنرجو (شامل شماره تماس/ایمیل)</summary>
    [HttpGet("{studentProfileId:guid}")]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid studentProfileId)
    {
        var student = await _studentDirectoryService.GetStudentByIdAsync(studentProfileId);
        if (student is null)
            return NotFound(new { errors = new[] { "هنرجویی با این شناسه یافت نشد." } });

        return Ok(student);
    }
}
