namespace CityChoir.Application.DTOs.Attendance;

public class AttendanceDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string UserFullName { get; set; }
    public int? RehearsalId { get; set; }
    public string RehearsalName { get; set; }
    public bool IsPresent { get; set; }
    public DateTime MarkedAt { get; set; }
    public string? Notes { get; set; }
}