using CityChoir.Application.DTOs.Auth;
using CityChoir.Application.DTOs.Common;

namespace CityChoir.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<string>> Register(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>> Login(LoginDto dto);
    Task<ApiResponse<string>> VerifyEmail(string token);
    Task<ApiResponse<string>> ResendVerification(string email);
    Task<ApiResponse<string>> ForgotPassword(ForgotPasswordRequestDto dto);
    Task<ApiResponse<string>> ResetPassword(ResetPasswordDto dto);
}


