namespace CityChoir.Application.DTOs.Attendance;

public class AttendanceReportDto
{
    public int TotalRehearsals { get; set; }
    public int TotalMembers { get; set; }
    public int TotalPresent { get; set; }
    public int TotalAbsent { get; set; }
    public double OverallAttendancePercentage { get; set; }

    public IEnumerable<PartAttendanceDto> ByPart { get; set; }
    public IEnumerable<GenderAttendanceDto> ByGender { get; set; }
}