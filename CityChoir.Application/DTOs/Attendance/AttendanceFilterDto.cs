using CityChoir.Domain.Enums;

namespace CityChoir.Application.DTOs.Attendance;

public class AttendanceFilterDto
{
    public int? RehearsalId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public ChoirPart? Part { get; set; }
    public string? Gender { get; set; }
}