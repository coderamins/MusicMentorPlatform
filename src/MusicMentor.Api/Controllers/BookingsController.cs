using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicMentor.Api.Common;
using MusicMentor.Application.DTOs.Bookings;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Enums;

namespace MusicMentor.Api.Controllers;

[ApiController]
[Route("api/v1/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>هنرآموز درخواست رزرو یک جلسه با یک استاد را ثبت می‌کند</summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.Student)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var result = await _bookingService.CreateAsync(User.GetUserId(), request);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>استاد درخواست رزرو را تایید می‌کند؛ وضعیت به AwaitingPayment تغییر می‌کند</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] BookingActionRequest request)
    {
        var result = await _bookingService.ApproveAsync(User.GetUserId(), id, request);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>استاد درخواست رزرو را رد می‌کند</summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] BookingActionRequest request)
    {
        var result = await _bookingService.RejectAsync(User.GetUserId(), id, request);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>هنرآموز یا استاد، رزروی که هنوز نهایی نشده را لغو می‌کند</summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] BookingActionRequest request)
    {
        var result = await _bookingService.CancelAsync(User.GetUserId(), id, request);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>لیست رزروهای کاربر جاری (چه به‌عنوان هنرآموز چه به‌عنوان استاد)</summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var bookings = await _bookingService.GetMineAsync(User.GetUserId());
        return Ok(bookings);
    }

    /// <summary>جزئیات یک رزرو - فقط برای طرفین همان رزرو</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _bookingService.GetByIdAsync(User.GetUserId(), id);
        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }
}
