using CityChoir.Application.DTOs.Common;
using CityChoir.Application.DTOs.Rehearsal;
using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;

namespace CityChoir.Application.Services;

public class RehearsalService : IRehearsalService
{
    private readonly IRehearsalRepository _rehearsalRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public RehearsalService(
        IRehearsalRepository rehearsalRepository,
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _rehearsalRepository = rehearsalRepository;
        _userRepository = userRepository;
        _emailService = emailService;
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

        var activeMembers = await _userRepository.GetActiveMembers();
        var subject = "New rehearsal scheduled";
        var body = $@"
            <p>A new rehearsal has been created:</p>
            <ul>
                <li><strong>{rehearsal.Name}</strong></li>
                <li>{rehearsal.Description}</li>
                <li>When: {rehearsal.RehearsalDate:MMMM dd, yyyy}</li>
                <li>Start: {rehearsal.StartTime:HH:mm}</li>
                <li>End: {rehearsal.EndTime:HH:mm}</li>
                <li>Location: {rehearsal.Lat}, {rehearsal.Lng} (radius {rehearsal.RadiusMeters} meters)</li>
            </ul>
        ";

        // Send emails concurrently to avoid long sequential waits
        var emailTasks = new List<Task>();
        foreach (var member in activeMembers)
        {
            emailTasks.Add(_emailService.SendEmailAsync(member.Email, subject, body));
        }

        try
        {
            await Task.WhenAll(emailTasks);
        }
        catch
        {
            // If one or more emails fail, swallow exceptions to avoid failing the whole operation.
            // Individual failures are logged by the EmailService fallback.
        }

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
