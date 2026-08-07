using CityChoir.Application.DTOs.Admin;
using CityChoir.Application.DTOs.Common;

namespace CityChoir.Application.Interfaces;

public interface IAdminUserService
{
    Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsers();
    Task<ApiResponse<UserDto>> GetUserById(Guid id);
    Task<ApiResponse<string>> AssignRole(AssignRoleDto dto);
    Task<ApiResponse<string>> DeleteUser(Guid id);
}