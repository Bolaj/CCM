using CityChoir.Domain.Enums;

namespace CityChoir.Application.DTOs.Admin;

public class AssignRoleDto
{
    public required string UserId { get; set; }
    public required UserRole Role { get; set; }
}