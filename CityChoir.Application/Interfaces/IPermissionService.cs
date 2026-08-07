using CityChoir.Application.DTOs.Common;
using CityChoir.Application.DTOs.Permission;

namespace CityChoir.Application.Interfaces;

public interface IPermissionService
{
    Task<ApiResponse<IEnumerable<PermissionDto>>> GetAll();
    Task<ApiResponse<PermissionDto>> GetById(Guid id);
    Task<ApiResponse<string>> Approve(Guid id);
    Task<ApiResponse<string>> Decline(Guid id, ReviewPermissionDto dto);
    Task<ApiResponse<string>> RequestPermission(CreatePermissionDto dto);
    Task<ApiResponse<IEnumerable<PermissionDto>>> GetByUserId(Guid userId);
}