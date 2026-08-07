using System.Security.Claims;
using CityChoir.Application.DTOs.Attendance;
using CityChoir.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CityChoir.API.Controllers;

[ApiController]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("mark")]
    public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // extract userId from JWT token
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("User not found in token");

        dto.UserId = Guid.Parse(userIdClaim);

        var result = await _attendanceService.MarkAttendance(dto);
        return Ok(result);
    }
}