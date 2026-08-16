using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicMentor.Api.Common;
using MusicMentor.Application.DTOs.Teachers;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Enums;

namespace MusicMentor.Api.Controllers;

[ApiController]
[Route("api/v1/teachers/me/location")]
[Authorize(Roles = UserRoles.Teacher)]
public class TeacherLocationController : ControllerBase
{
    private readonly ITeacherLocationService _locationService;

    public TeacherLocationController(ITeacherLocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// ثبت/به‌روزرسانی موقعیت جغرافیایی استاد (معمولاً از GPS مرورگر گرفته می‌شه).
    /// این موقعیت برای جستجوی «اساتید نزدیک من» توسط هنرآموزها استفاده می‌شه.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateTeacherLocationRequest request)
    {
        var result = await _locationService.UpdateLocationAsync(User.GetUserId(), request.Latitude, request.Longitude);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
