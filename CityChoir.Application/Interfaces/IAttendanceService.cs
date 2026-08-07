using CityChoir.Application.DTOs.Attendance;
using CityChoir.Application.DTOs.Common;

namespace CityChoir.Application.Interfaces;

public interface IAttendanceService
{
    Task<ApiResponse<string>> MarkAttendance(MarkAttendanceDto dto);

    Task<ApiResponse<AttendanceReportDto>> GetReport(AttendanceFilterDto filter);
}