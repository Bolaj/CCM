using CityChoir.Application.DTOs.Common;
using CityChoir.Application.DTOs.Permission;
using CityChoir.Application.Interfaces;
using CityChoir.Domain.Enums;

namespace CityChoir.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRehearsalRepository _rehearsalRepository;
    private readonly IEmailService _emailService;

    public PermissionService(IPermissionRepository permissionRepository, IUserRepository userRepository, IRehearsalRepository rehearsalRepository, IEmailService emailService)
    {
        _permissionRepository = permissionRepository;
        _userRepository = userRepository;
        _rehearsalRepository = rehearsalRepository;
        _emailService = emailService;
    }
    public async Task<ApiResponse<IEnumerable<PermissionDto>>> GetAll()
    {
        var permissions = await _permissionRepository.GetAll();
        var dtos = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            UserId = p.UserId.ToString(),
            UserFullName = p.User.FullName,
            RehearsalId = p.RehearsalId,
            RehearsalName = p.Rehearsal.Name,
            Reason = p.Reason,
            Status = p.Status,
            DeclineReason = p.DeclineReason,
            RequestedAt = p.RequestedAt,
            ReviewedAt = p.ReviewedAt
        });

        return ApiResponse<IEnumerable<PermissionDto>>.SuccessResponse("All Permission Requests:", dtos);
    }

    public async Task<ApiResponse<PermissionDto>> GetById(Guid id)
    {
        var p = await _permissionRepository.GetById(id);
        if (p == null)
            return ApiResponse<PermissionDto>.FailureResponse("Permission request not found");

        var dto = new PermissionDto
        {
            Id = p.Id,
            UserId = p.UserId.ToString(),
            UserFullName = p.User.FullName,
            RehearsalId = p.RehearsalId,
            RehearsalName = p.Rehearsal.Name,
            Reason = p.Reason,
            Status = p.Status,
            DeclineReason = p.DeclineReason,
            RequestedAt = p.RequestedAt,
            ReviewedAt = p.ReviewedAt
        };

        return ApiResponse<PermissionDto>.SuccessResponse("Permission Found for: ", dto);
    }

    public async Task<ApiResponse<string>> Approve(Guid id)
    {
        var permission = await _permissionRepository.GetById(id);
        if (permission == null)
            return ApiResponse<string>.FailureResponse("Permission request not found");

        if (permission.Status != PermissionStatus.PENDING)
            return ApiResponse<string>.FailureResponse("Your permission is pending. You will be notified upon approval");

        permission.Status = PermissionStatus.APPROVED;
        permission.ReviewedAt = DateTime.UtcNow;
        await _permissionRepository.Update(permission);

        return ApiResponse<string>.SuccessResponse("Permission approved", permission.Id.ToString());
    }

    public async Task<ApiResponse<string>> Decline(Guid id, ReviewPermissionDto dto)
    {
        var permission = await _permissionRepository.GetById(id);
        if (permission == null)
            return ApiResponse<string>.FailureResponse("Permission request not found");

        if (permission.Status != PermissionStatus.PENDING)
            return ApiResponse<string>.FailureResponse("Only pending requests can be declined");

        if (string.IsNullOrEmpty(dto.DeclineReason))
            return ApiResponse<string>.FailureResponse("Decline reason is required");

        permission.Status = PermissionStatus.DECLINED;
        permission.DeclineReason = dto.DeclineReason;
        permission.ReviewedAt = DateTime.UtcNow;
        await _permissionRepository.Update(permission);

        return ApiResponse<string>.SuccessResponse("Permission declined", permission.Id.ToString());
    }

    public async Task<ApiResponse<string>> RequestPermission(CreatePermissionDto dto)
    {
        var user = await _userRepository.GetById(dto.UserId);
        if (user == null)
            return ApiResponse<string>.FailureResponse("User not found");

        var rehearsal = await _rehearsalRepository.GetById(dto.RehearsalId);
        if (rehearsal == null)
            return ApiResponse<string>.FailureResponse("Rehearsal not found");

        var permission = new Domain.Entities.Permission
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            RehearsalId = dto.RehearsalId,
            Reason = dto.Reason,
            Status = PermissionStatus.PENDING,
            RequestedAt = DateTime.UtcNow
        };

        await _permissionRepository.Add(permission);

        await _emailService.SendEmailAsync(
            user.Email,
            "Permission Request Received",
            $"""
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
            <h2 style="color: #2196F3;">Permission Request Received ⏳</h2>
            <p>Hi <strong>{user.FullName}</strong>,</p>
            <p>Your permission request has been received and is currently <strong>pending review</strong>.</p>
            <table style="width: 100%; border-collapse: collapse; margin: 20px 0;">
                <tr>
                    <td style="padding: 8px; border: 1px solid #ddd;"><strong>Rehearsal</strong></td>
                    <td style="padding: 8px; border: 1px solid #ddd;">{rehearsal.Name}</td>
                </tr>
                <tr>
                    <td style="padding: 8px; border: 1px solid #ddd;"><strong>Reason</strong></td>
                    <td style="padding: 8px; border: 1px solid #ddd;">{dto.Reason}</td>
                </tr>
                <tr>
                    <td style="padding: 8px; border: 1px solid #ddd;"><strong>Submitted At</strong></td>
                    <td style="padding: 8px; border: 1px solid #ddd;">{permission.RequestedAt:dddd, MMMM dd yyyy HH:mm} UTC</td>
                </tr>
            </table>
            <p>You will be notified once your request has been reviewed.</p>
            <p style="color: #888; font-size: 12px;">CityChoir — This is an automated message, please do not reply.</p>
        </div>
        """
        );

        return ApiResponse<string>.SuccessResponse("Permission request submitted successfully", null);
    }
    public async Task<ApiResponse<IEnumerable<PermissionDto>>> GetByUserId(Guid userId)
    {
        var permissions = await _permissionRepository.GetAll();
        var userPermissions = permissions.Where(p => p.UserId == userId);

        var dtos = userPermissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            UserId = p.UserId.ToString(),
            UserFullName = p.User.FullName,
            RehearsalId = p.RehearsalId,
            RehearsalName = p.Rehearsal.Name,
            Reason = p.Reason,
            Status = p.Status,
            DeclineReason = p.DeclineReason,
            RequestedAt = p.RequestedAt,
            ReviewedAt = p.ReviewedAt
        });

        return ApiResponse<IEnumerable<PermissionDto>>.SuccessResponse("User permissions fetched", dtos);
    }
}