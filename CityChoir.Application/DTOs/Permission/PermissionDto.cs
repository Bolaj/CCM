using CityChoir.Domain.Enums;

namespace CityChoir.Application.DTOs.Permission;

public class PermissionDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string UserFullName { get; set; }
    public int RehearsalId { get; set; }
    public string RehearsalName { get; set; }
    public string Reason { get; set; }
    public PermissionStatus Status { get; set; }
    public string? DeclineReason { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}