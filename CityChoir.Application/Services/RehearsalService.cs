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

    public async Task<ApiResponse<IEnumerable<Rehearsal>>> GetAll()
{
    var rehearsals = await _rehearsalRepository.GetAll();
    return ApiResponse<IEnumerable<Rehearsal>>.SuccessResponse("Rehearsals fetched successfully", rehearsals);
}

    public async Task<ApiResponse<Rehearsal?>> GetById(int id)
    {
        var rehearsal = await _rehearsalRepository.GetById(id);
        if (rehearsal == null)
            return ApiResponse<Rehearsal?>.FailureResponse("Rehearsal not found");

        return ApiResponse<Rehearsal?>.SuccessResponse("Rehearsal fetched successfully", rehearsal);
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
            .Where(rehearsal => upcoming
                ? rehearsal.StartTime > now
                : rehearsal.EndTime <= now)
            .OrderBy(rehearsal => upcoming ? rehearsal.StartTime : rehearsal.EndTime)
            .Select(rehearsal => new UserRehearsalDto
            {
                Id = rehearsal.Id,
                Name = rehearsal.Name,
                Description = rehearsal.Description,
                Lat = rehearsal.Lat,
                Lng = rehearsal.Lng,
                RadiusMeters = rehearsal.RadiusMeters,
                RehearsalDate = rehearsal.RehearsalDate,
                StartTime = rehearsal.StartTime,
                EndTime = rehearsal.EndTime,
                HasAttended = attendances.TryGetValue(rehearsal.Id, out var attendance)
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
}
