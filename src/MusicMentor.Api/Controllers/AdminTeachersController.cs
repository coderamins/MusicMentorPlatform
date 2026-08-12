using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicMentor.Application.DTOs.Admin;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Enums;

namespace MusicMentor.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/teachers")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminTeachersController : ControllerBase
{
    private readonly IAdminTeacherService _adminTeacherService;

    public AdminTeachersController(IAdminTeacherService adminTeacherService)
    {
        _adminTeacherService = adminTeacherService;
    }

    /// <summary>
    /// لیست اساتید برای بررسی ادمین. با status می‌تونی فیلتر کنی؛ اگه ندی، پیش‌فرض PendingReview نشون داده می‌شه.
    /// نمونه: /api/v1/admin/teachers?status=PendingReview&amp;page=1&amp;pageSize=20
    /// برای دیدن همه (صرف‌نظر از وضعیت): /api/v1/admin/teachers?status=All
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        TeacherApprovalStatus? parsedStatus = TeacherApprovalStatus.PendingReview;

        if (string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
        {
            parsedStatus = null;
        }
        else if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<TeacherApprovalStatus>(status, ignoreCase: true, out var s))
                return BadRequest(new { errors = new[] { "مقدار status نامعتبر است. مقادیر مجاز: PendingReview, Approved, Rejected, All" } });

            parsedStatus = s;
        }

        var result = await _adminTeacherService.SearchAsync(parsedStatus, page, pageSize);
        return Ok(result);
    }

    /// <summary>جزئیات کامل یک استاد برای بررسی: بیوگرافی (سابقه کاری/تدریسی)، سابقه به سال، حوزه‌های تدریس</summary>
    [HttpGet("{teacherProfileId:guid}")]
    [ProducesResponseType(typeof(AdminTeacherDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid teacherProfileId)
    {
        var teacher = await _adminTeacherService.GetByIdAsync(teacherProfileId);
        if (teacher is null)
            return NotFound(new { errors = new[] { "استاد پیدا نشد." } });

        return Ok(teacher);
    }

    /// <summary>تایید استاد - از این پس در جستجوی عمومی نمایش داده می‌شود و می‌تواند رزرو بپذیرد</summary>
    [HttpPost("{teacherProfileId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid teacherProfileId)
    {
        var result = await _adminTeacherService.ApproveAsync(teacherProfileId);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>رد درخواست استاد؛ ذکر دلیل (reason) الزامی است</summary>
    [HttpPost("{teacherProfileId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid teacherProfileId, [FromBody] RejectTeacherRequest request)
    {
        var result = await _adminTeacherService.RejectAsync(teacherProfileId, request.Reason);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
