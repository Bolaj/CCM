namespace CityChoir.Application.DTOs.Permission;

public class CreatePermissionDto
{
    public required string UserId { get; set; }
    public required int RehearsalId { get; set; }
    public required string Reason { get; set; }
}