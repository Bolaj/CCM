using CityChoir.Application.DTOs.Attendance;
using CityChoir.Application.DTOs.Common;
using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;
using CityChoir.Domain.Enums;

namespace CityChoir.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IRehearsalRepository _rehearsalRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IRehearsalRepository rehearsalRepository,
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _attendanceRepository = attendanceRepository;
        _rehearsalRepository = rehearsalRepository;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<ApiResponse<string>> MarkAttendance(MarkAttendanceDto dto)
    {
        // 1. find active rehearsal
        var rehearsals = await _rehearsalRepository.GetAll();
        var now = DateTime.UtcNow;
        var active = rehearsals.FirstOrDefault(r => r.StartTime <= now && r.EndTime >= now);

        if (active == null)
            return ApiResponse<string>.FailureResponse("No active rehearsal at this time");

        // 2. get user
        var user = await _userRepository.GetById(dto.UserId);
        if (user == null)
            return ApiResponse<string>.FailureResponse("User not found");

        // 3. check already marked
        var existing = await _attendanceRepository.GetByUserAndRehearsal(dto.UserId, active.Id);
        if (existing != null)
            return ApiResponse<string>.FailureResponse("Attendance already marked for this rehearsal");

        // 4. haversine check
        var distance = HaversineDistance(dto.Lat, dto.Lng, active.Lat, active.Lng);
        if (distance > active.RadiusMeters)
            return ApiResponse<string>.FailureResponse($"You are not within the rehearsal venue. You are {distance:F0}m away");

        // 5. save attendance
        var attendance = new Attendance
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            RehearsalId = active.Id,
            IsPresent = true,
            MarkedAt = DateTime.UtcNow
        };

        await _attendanceRepository.Add(attendance);

        //6. send confirmation email
        await _emailService.SendEmailAsync(
            user.Email,
            "Attendance Confirmed ✅",
            $"""
             <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                 <h2 style="color: #4CAF50;">Attendance Confirmed ✅</h2>
                 <p>Hi <strong>{user.FullName}</strong>,</p>
                 <p>Your attendance for <strong>{active.Name}</strong> has been marked successfully.</p>
                 <table style="width: 100%; border-collapse: collapse; margin: 20px 0;">
                     <tr>
                         <td style="padding: 8px; border: 1px solid #ddd;"><strong>Rehearsal</strong></td>
                         <td style="padding: 8px; border: 1px solid #ddd;">{active.Name}</td>
                     </tr>
                     <tr>
                         <td style="padding: 8px; border: 1px solid #ddd;"><strong>Date</strong></td>
                         <td style="padding: 8px; border: 1px solid #ddd;">{active.RehearsalDate:dddd, MMMM dd yyyy}</td>
                     </tr>
                     <tr>
                         <td style="padding: 8px; border: 1px solid #ddd;"><strong>Time Marked</strong></td>
                         <td style="padding: 8px; border: 1px solid #ddd;">{attendance.MarkedAt:HH:mm} UTC</td>
                     </tr>
                 </table>
                 <p style="color: #888; font-size: 12px;">CityChoir — This is an automated message, please do not reply.</p>
             </div>
             """
        );

        return ApiResponse<string>.SuccessResponse("Attendance marked successfully", dto.ToString());
    }
    
    public async Task<ApiResponse<AttendanceReportDto>> GetReport(AttendanceFilterDto filter)
    {
        var attendances = (await _attendanceRepository.GetAll(filter)).ToList();
        var totalRehearsals = await _attendanceRepository.GetTotalRehearsalsCount(filter);
        var totalMembers = attendances.Select(a => a.UserId).Distinct().Count();

        var report = new AttendanceReportDto
        {
            TotalRehearsals = totalRehearsals,
            TotalMembers = totalMembers,
            TotalPresent = attendances.Count(a => a.IsPresent),
            TotalAbsent = attendances.Count(a => !a.IsPresent),
            OverallAttendancePercentage = attendances.Count == 0 ? 0 :
                Math.Round((double)attendances.Count(a => a.IsPresent) / attendances.Count * 100, 1),

            ByPart = Enum.GetValues<ChoirPart>().Select(part =>
            {
                var partAttendances = attendances.Where(a => a.User.Part == part).ToList();
                var present = partAttendances.Count(a => a.IsPresent);
                var total = partAttendances.Count;
                return new PartAttendanceDto
                {
                    Part = part,
                    Present = present,
                    Absent = total - present,
                    Total = total,
                    AttendancePercentage = total == 0 ? 0 : Math.Round((double)present / total * 100, 1)
                };
            }),

            ByGender = attendances
                .GroupBy(a => a.User.Gender)
                .Select(g =>
                {
                    var present = g.Count(a => a.IsPresent);
                    var total = g.Count();
                    return new GenderAttendanceDto
                    {
                        Gender = g.Key,
                        Present = present,
                        Absent = total - present,
                        Total = total,
                        AttendancePercentage = total == 0 ? 0 : Math.Round((double)present / total * 100, 1)
                    };
                })
        };

        return ApiResponse<AttendanceReportDto>.SuccessResponse("Attendance Report:",report);
    }
    
    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth radius in metres
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
    
    
}