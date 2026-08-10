using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicMentor.Api.Common;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Enums;

namespace MusicMentor.Api.Controllers;

[ApiController]
[Route("api/v1/teachers/me")]
[Authorize(Roles = UserRoles.Teacher)]
public class TeacherOnboardingController : ControllerBase
{
    private readonly ITeacherOnboardingService _onboardingService;

    public TeacherOnboardingController(ITeacherOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    /// <summary>
    /// آپلود (یا جایگزینی) فایل رزومه/مدارک. فرم multipart/form-data با یک فیلد فایل به نام "resume".
    /// فرمت مجاز: PDF/DOC/DOCX، حداکثر حجم ۵ مگابایت.
    /// </summary>
    [HttpPost("resume")]
    [RequestSizeLimit(6_000_000)] // کمی بیشتر از حد مجاز ۵MB برای overhead خود multipart
    public async Task<IActionResult> UploadResume([FromForm] IFormFile resume)
    {
        if (resume is null || resume.Length == 0)
            return BadRequest(new { errors = new[] { "فایل رزومه ارسال نشده است." } });

        await using var stream = resume.OpenReadStream();
        var result = await _onboardingService.UploadResumeAsync(
            User.GetUserId(), stream, resume.FileName, resume.Length);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>وضعیت فعلی بررسی پروفایل توسط ادمین (PendingReview / Approved / Rejected)</summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _onboardingService.GetMyStatusAsync(User.GetUserId());
        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
