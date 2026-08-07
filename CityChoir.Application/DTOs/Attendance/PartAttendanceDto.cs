using CityChoir.Domain.Enums;

namespace CityChoir.Application.DTOs.Attendance;

public class PartAttendanceDto
{
    public ChoirPart Part { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Total { get; set; }
    public double AttendancePercentage { get; set; }
}