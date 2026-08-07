using CityChoir.Application.DTOs.Common;
using CityChoir.Application.DTOs.Permission;
using CityChoir.Application.Interfaces;
using CityChoir.Domain.Enums;

namespace CityChoir.Application.Services;

public class PermissionService: IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
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
        if (!Guid.TryParse(dto.UserId, out var userId))
            return ApiResponse<string>.FailureResponse("Invalid user id");

        var permission = new Domain.Entities.Permission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RehearsalId = dto.RehearsalId,
            Reason = dto.Reason,
            Status = PermissionStatus.PENDING,
            RequestedAt = DateTime.UtcNow
        };

        await _permissionRepository.Add(permission);

        return ApiResponse<string>.SuccessResponse("Permission requested", permission.Id.ToString());
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