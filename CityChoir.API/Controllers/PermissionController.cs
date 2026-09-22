// CityChoir.API/Controllers/PermissionController.cs
using System.Security.Claims;
using CityChoir.Application.DTOs.Permission;
using CityChoir.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CityChoir.API.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RequestPermission([FromBody] CreatePermissionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("User not found in token");

        dto.UserId = Guid.Parse(userIdClaim);

        var result = await _permissionService.RequestPermission(dto);
        return Ok(result);
    }

    [HttpGet("my/{userId}")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetMyPermissions(Guid userId)
    {
        var result = await _permissionService.GetByUserId(userId);
        return Ok(result);
    }
}