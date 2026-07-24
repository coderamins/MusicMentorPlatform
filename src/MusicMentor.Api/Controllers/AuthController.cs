using Microsoft.AspNetCore.Mvc;
using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.Interfaces;

namespace MusicMentor.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>ثبت‌نام هنرآموز (کسی که به دنبال استاد است)</summary>
    [HttpPost("register/student")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentRequest request)
    {
        var result = await _authService.RegisterStudentAsync(request);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>ثبت‌نام استاد موسیقی</summary>
    [HttpPost("register/teacher")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterTeacher([FromBody] RegisterTeacherRequest request)
    {
        var result = await _authService.RegisterTeacherAsync(request);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>ورود (مشترک بین استاد و هنرآموز) - با ایمیل یا موبایل</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Succeeded)
            return Unauthorized(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
