using CityChoir.Application.DTOs.Admin;
using CityChoir.Application.DTOs.Common;
using CityChoir.Application.Interfaces;

namespace CityChoir.Application.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;

    public AdminUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _userRepository.GetActiveMembers();
        var dtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            RegistrationNumber = u.RegistrationNumber,
            PhoneNumber = u.PhoneNumber,
            Gender = u.Gender,
            Role = u.Role,
            Part = u.Part,
            IsEmailVerified = u.IsEmailVerified,
            CreatedAt = u.CreatedAt
        });

        return ApiResponse<IEnumerable<UserDto>>.SuccessResponse("Users fetched successfully", dtos);
    }
    
    public async Task<ApiResponse<UserDto>> GetUserById(Guid id)
    {
        var user = await _userRepository.GetById(id);
        if (user == null)
            return ApiResponse<UserDto>.FailureResponse("User not found");

        var dto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RegistrationNumber = user.RegistrationNumber,
            PhoneNumber = user.PhoneNumber,
            Gender = user.Gender,
            Role = user.Role,
            Part = user.Part,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<UserDto>.SuccessResponse("User fetched successfully", dto);
    }

    public async Task<ApiResponse<string>> AssignRole(AssignRoleDto dto)
    {
        var user = await _userRepository.GetById(Guid.Parse(dto.UserId));
        if (user == null)
            return ApiResponse<string>.FailureResponse("User not found");

        user.Role = dto.Role;
        await _userRepository.Update(user);

        return ApiResponse<string>.SuccessResponse("Role assigned successfully", user.Id.ToString());
    }
    
    public async Task<ApiResponse<string>> DeleteUser(Guid id)
    {
        var user = await _userRepository.GetById(id);
        if (user == null)
            return ApiResponse<string>.FailureResponse("User not found");

        // soft delete — just flag or remove based on your preference
        // for now hard delete via repo if you have it, or add a DeleteById method
        return ApiResponse<string>.FailureResponse("Not implemented yet");
    }
}