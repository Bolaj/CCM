using System.Security.Claims;
using CityChoir.Application.DTOs.Admin;
using CityChoir.Application.DTOs.Attendance;
using CityChoir.Application.DTOs.Permission;
using CityChoir.Application.DTOs.Rehearsal;
using CityChoir.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CityChoir.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "ADMIN,SUPER_ADMIN")]
public class AdminController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;
    private readonly IRehearsalService _rehearsalService;
    private readonly IPermissionService _permissionService;
    private readonly IAttendanceService _attendanceService;

    public AdminController(
        IAdminUserService adminUserService,
        IRehearsalService rehearsalService,
        IPermissionService permissionService,
        IAttendanceService attendanceService)
    {
        _adminUserService = adminUserService;
        _rehearsalService = rehearsalService;
        _permissionService = permissionService;
        _attendanceService = attendanceService;
    }

    // ── Users ──────────────────────────────────────────────
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _adminUserService.GetAllUsers();
        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _adminUserService.GetUserById(id);
        return Ok(result);
    }

    [HttpPut("users/assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminUserService.AssignRole(dto);
        return Ok(result);
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _adminUserService.DeleteUser(id);
        return Ok(result);
    }

    // ── Rehearsals ─────────────────────────────────────────
    [HttpGet("rehearsals")]
    public async Task<IActionResult> GetAllRehearsals()
    {
        var result = await _rehearsalService.GetAll();
        return Ok(result);
    }

    [HttpGet("rehearsals/{id}")]
    public async Task<IActionResult> GetRehearsalById(int id)
    {
        var result = await _rehearsalService.GetById(id);
        return Ok(result);
    }

    [HttpPost("rehearsals")]
    public async Task<IActionResult> CreateRehearsal([FromBody] CreateRehearsalDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _rehearsalService.Create(dto);
        return Ok(result);
    }

    [HttpPut("rehearsals")]
    public async Task<IActionResult> UpdateRehearsal([FromBody] UpdateRehearsalDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _rehearsalService.Update(dto);
        return Ok(result);
    }

    [HttpDelete("rehearsals/{id}")]
    public async Task<IActionResult> DeleteRehearsal(int id)
    {
        var result = await _rehearsalService.Delete(id);
        return Ok(result);
    }

    // ── Permissions ────────────────────────────────────────
    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _permissionService.GetAll();
        return Ok(result);
    }

    [HttpGet("permissions/{id}")]
    public async Task<IActionResult> GetPermissionById(Guid id)
    {
        var result = await _permissionService.GetById(id);
        return Ok(result);
    }

    [HttpPut("permissions/{id}/approve")]
    public async Task<IActionResult> ApprovePermission(Guid id)
    {
        var result = await _permissionService.Approve(id);
        return Ok(result);
    }

    [HttpPut("permissions/{id}/decline")]
    public async Task<IActionResult> DeclinePermission(Guid id, [FromBody] ReviewPermissionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _permissionService.Decline(id, dto);
        return Ok(result);
    }

    // ── Attendance ─────────────────────────────────────────
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceReport([FromQuery] AttendanceFilterDto filter)
    {
        var result = await _attendanceService.GetReport(filter);
        return Ok(result);
    }
}