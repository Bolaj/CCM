using CityChoir.Domain.Enums;

namespace CityChoir.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int RehearsalId { get; set; }
    public string Reason { get; set; }
    public PermissionStatus Status { get; set; } = PermissionStatus.PENDING;
    public string? DeclineReason { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public User User { get; set; }
    public Rehearsal Rehearsal { get; set; }
}