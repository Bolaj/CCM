namespace CityChoir.Application.DTOs.Attendance;

public class MarkAttendanceDto
{
    public  Guid UserId { get; set; }
    public required double Lat { get; set; }
    public required double Lng { get; set; }
}