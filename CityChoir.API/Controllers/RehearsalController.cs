using System.Security.Claims;
using CityChoir.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CityChoir.API.Controllers;

[ApiController]
[Route("api/rehearsals")]
[Authorize]
public class RehearsalController : ControllerBase
{
    private readonly IRehearsalService _rehearsalService;

    public RehearsalController(IRehearsalService rehearsalService)
    {
        _rehearsalService = rehearsalService;
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("User not found in token");

        return Ok(await _rehearsalService.GetUpcomingForUser(userId.Value));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("User not found in token");

        return Ok(await _rehearsalService.GetHistoryForUser(userId.Value));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var userId) ? userId : null;
    }
}