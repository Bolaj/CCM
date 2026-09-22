using CityChoir.Application.DTOs.Common;
using CityChoir.Application.DTOs.Rehearsal;
using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;

namespace CityChoir.Application.Services;

public class RehearsalService : IRehearsalService
{
    private readonly IRehearsalRepository _rehearsalRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IRehearsalNotificationQueue _notificationQueue;

    public RehearsalService(
        IRehearsalRepository rehearsalRepository,
        IAttendanceRepository attendanceRepository,
        IRehearsalNotificationQueue notificationQueue)
    {
        _rehearsalRepository = rehearsalRepository;
        _attendanceRepository = attendanceRepository;
        _notificationQueue = notificationQueue;
    }

    public async Task<ApiResponse<IEnumerable<RehearsalDto>>> GetAll()
    {
        var rehearsals = await _rehearsalRepository.GetAll();
        var result = rehearsals.Select(ToDto);

        return ApiResponse<IEnumerable<RehearsalDto>>.SuccessResponse(
            "Rehearsals fetched successfully",
            result);
    }

    public async Task<ApiResponse<RehearsalDto?>> GetById(int id)
    {
        var rehearsal = await _rehearsalRepository.GetById(id);
        if (rehearsal == null)
            return ApiResponse<RehearsalDto?>.FailureResponse("Rehearsal not found");

        return ApiResponse<RehearsalDto?>.SuccessResponse(
            "Rehearsal fetched successfully",
            ToDto(rehearsal));
    }

    public Task<ApiResponse<IEnumerable<UserRehearsalDto>>> GetUpcomingForUser(Guid userId)
    {
        return GetUserRehearsals(userId, upcoming: true);
    }

    public Task<ApiResponse<IEnumerable<UserRehearsalDto>>> GetHistoryForUser(Guid userId)
    {
        return GetUserRehearsals(userId, upcoming: false);
    }

    private async Task<ApiResponse<IEnumerable<UserRehearsalDto>>> GetUserRehearsals(
        Guid userId,
        bool upcoming)
    {
        var now = DateTime.UtcNow;
        var rehearsals = await _rehearsalRepository.GetAll();
        var attendances = (await _attendanceRepository.GetByUserId(userId))
            .ToDictionary(attendance => attendance.RehearsalId);

        var selected = rehearsals
            .Select(rehearsal => new
            {
                Rehearsal = rehearsal,
                Start = RehearsalSchedule.GetStart(rehearsal),
                End = RehearsalSchedule.GetEnd(rehearsal)
            })
            .Where(item => upcoming ? item.Start > now : item.End <= now)
            .OrderBy(item => upcoming ? item.Start : item.End)
            .Select(item => new UserRehearsalDto
            {
                Id = item.Rehearsal.Id,
                Name = item.Rehearsal.Name,
                Description = item.Rehearsal.Description,
                Venue = item.Rehearsal.Venue,
                Lat = item.Rehearsal.Lat,
                Lng = item.Rehearsal.Lng,
                RadiusMeters = item.Rehearsal.RadiusMeters,
                RehearsalDate = item.Rehearsal.RehearsalDate,
                StartTime = item.Rehearsal.StartTime,
                EndTime = item.Rehearsal.EndTime,
                HasAttended = attendances.TryGetValue(item.Rehearsal.Id, out var attendance)
                    && attendance.IsPresent,
                AttendedAt = attendance?.MarkedAt
            });

        var message = upcoming
            ? "Upcoming rehearsals fetched successfully"
            : "Rehearsal history fetched successfully";

        return ApiResponse<IEnumerable<UserRehearsalDto>>.SuccessResponse(message, selected);
    }

    public async Task<ApiResponse<string>> Create(CreateRehearsalDto rehearsalDto)
    {
        var rehearsal = new Rehearsal
        {
            Name = rehearsalDto.Name,
            Description = rehearsalDto.Description,
            Venue = rehearsalDto.Venue,
            Lat = rehearsalDto.Lat,
            Lng = rehearsalDto.Lng,
            RadiusMeters = rehearsalDto.RadiusMeters,
            StartTime = rehearsalDto.StartTime,
            EndTime = rehearsalDto.EndTime,
            RehearsalDate = rehearsalDto.RehearsalDate
        };

        await _rehearsalRepository.Add(rehearsal);

        await _notificationQueue.EnqueueAsync(new RehearsalNotificationDto
        {
            Name = rehearsal.Name,
            Description = rehearsal.Description,
            Venue = rehearsal.Venue,
            Lat = rehearsal.Lat,
            Lng = rehearsal.Lng,
            RadiusMeters = rehearsal.RadiusMeters,
            StartTime = rehearsal.StartTime,
            EndTime = rehearsal.EndTime,
            RehearsalDate = rehearsal.RehearsalDate
        });

        return ApiResponse<string>.SuccessResponse("Rehearsal created successfully", null);
    }

    public async Task<ApiResponse<string>> Update(UpdateRehearsalDto rehearsalDto)
    {
        var existing = await _rehearsalRepository.GetById(rehearsalDto.Id);
        if (existing == null)
            return ApiResponse<string>.FailureResponse("Rehearsal not found");

        existing.Name = rehearsalDto.Name;
        existing.Description = rehearsalDto.Description;
        existing.Venue = rehearsalDto.Venue;
        existing.Lat = rehearsalDto.Lat;
        existing.Lng = rehearsalDto.Lng;
        existing.RadiusMeters = rehearsalDto.RadiusMeters;
        existing.StartTime = rehearsalDto.StartTime;
        existing.EndTime = rehearsalDto.EndTime;
        existing.RehearsalDate = rehearsalDto.RehearsalDate;

        await _rehearsalRepository.Update(existing);

        return ApiResponse<string>.SuccessResponse("Rehearsal updated successfully", null);
    }

    public async Task<ApiResponse<string>> Delete(int id)
    {
        var existing = await _rehearsalRepository.GetById(id);
        if (existing == null)
            return ApiResponse<string>.FailureResponse("Rehearsal not found");

        await _rehearsalRepository.Delete(id);
        return ApiResponse<string>.SuccessResponse("Rehearsal deleted successfully", null);
    }

    public async Task<bool> Exists(int id)
    {
        return await _rehearsalRepository.Exists(id);
    }

    private static RehearsalDto ToDto(Rehearsal rehearsal)
    {
        return new RehearsalDto
        {
            Id = rehearsal.Id,
            Name = rehearsal.Name,
            Description = rehearsal.Description,
            Venue = rehearsal.Venue,
            Lat = rehearsal.Lat,
            Lng = rehearsal.Lng,
            RadiusMeters = rehearsal.RadiusMeters,
            RehearsalDate = rehearsal.RehearsalDate,
            StartTime = rehearsal.StartTime,
            EndTime = rehearsal.EndTime
        };
    }
}
