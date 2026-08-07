namespace CityChoir.Domain.Entities;

public class Attendance
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int RehearsalId { get; set; }
    public bool IsPresent { get; set; }
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public User User { get; set; }
    public Rehearsal Rehearsal { get; set; }
}