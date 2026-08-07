using CityChoir.Application.DTOs.Auth;
using CityChoir.Application.DTOs.Common;
using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;
using CityChoir.Domain.Enums;

namespace CityChoir.Application.Services;

public class AuthService : IAuthService
{
    private const string ADMIN_EMAIL = "admin@citychoir.com";
    private const string EMAIL_VERIFY_ROUTE = "/api/auth/verify-email?token={0}";
    private const string PASSWORD_RESET_ROUTE = "/api/auth/reset-password?token={0}";
    private readonly IUserRepository _userRepo;
    private readonly IEmailTokenRepository _tokenRepo;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepo,
        IEmailTokenRepository tokenRepo,
        IJwtService jwtService,
        IEmailService emailService)
    {
        _userRepo = userRepo;
        _tokenRepo = tokenRepo;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    public async Task<ApiResponse<string>> Register(RegisterDto dto)
    {
        if (await _userRepo.ExistsByEmail(dto.Email))
            return ApiResponse<string>.FailureResponse("Email already exists");

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var userRole = normalizedEmail.Equals(ADMIN_EMAIL, StringComparison.OrdinalIgnoreCase)
            ? UserRole.SUPER_ADMIN
            : UserRole.MEMBER;

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Address = dto.Address,
            Lga = dto.Lga,
            DateOfBirth = dto.DateOfBirth,
            Occupation = dto.Occupation,
            Gender = dto.Gender,
            PhoneNumber = dto.PhoneNumber,
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Part = dto.Part,
            Role = userRole,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.Add(user);

        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString(),
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            TokenType = EmailTokenType.EmailVerification
        };

        await _tokenRepo.Add(token);

        var link = string.Format(EMAIL_VERIFY_ROUTE, token.Token);

        await _emailService.SendEmailAsync(
            user.Email,
            "Verify your email",
            $"Click <a href='{link}'>here</a> to verify your email."
        );

        return ApiResponse<string>.SuccessResponse(
            "Registration successful. Check your email.",
            null
        );
    }

    public async Task<ApiResponse<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _userRepo.GetByEmail(dto.Email);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Invalid credentials");
        }

        if (!user.IsEmailVerified)
            return ApiResponse<AuthResponseDto>.FailureResponse("Email not verified");

        var token = _jwtService.GenerateToken(user);

        return ApiResponse<AuthResponseDto>.SuccessResponse(
            "Login successful",
            new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        );
    }

    public async Task<ApiResponse<string>> VerifyEmail(string token)
    {
        var record = await _tokenRepo.GetByToken(token);

        if (record == null || record.TokenType != EmailTokenType.EmailVerification)
            return ApiResponse<string>.FailureResponse("Invalid token");

        if (record.ExpiryDate < DateTime.UtcNow)
            return ApiResponse<string>.FailureResponse("Token expired");

        var user = await _userRepo.GetById(record.UserId);

        if (user == null)
            return ApiResponse<string>.FailureResponse("User not found");

        user.IsEmailVerified = true;

        await _userRepo.Update(user);
        await _tokenRepo.Delete(record);

        return ApiResponse<string>.SuccessResponse(
            "Email verified successfully",
            null
        );
    }

    public async Task<ApiResponse<string>> ResendVerification(string email)
    {
        var user = await _userRepo.GetByEmail(email);

        if (user == null)
            return ApiResponse<string>.FailureResponse("User not found");

        if (user.IsEmailVerified)
            return ApiResponse<string>.FailureResponse("Email already verified");

        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString(),
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            TokenType = EmailTokenType.EmailVerification
        };

        await _tokenRepo.Add(token);

        var link = string.Format(EMAIL_VERIFY_ROUTE, token.Token);

        await _emailService.SendEmailAsync(
            user.Email,
            "Verify your email",
            $"Click <a href='{link}'>here</a> to verify your email."
        );

        return ApiResponse<string>.SuccessResponse(
            "Verification email resent",
            null
        );
    }

    public async Task<ApiResponse<string>> ForgotPassword(ForgotPasswordRequestDto dto)
    {
        var user = await _userRepo.GetByEmail(dto.Email);

        if (user == null)
            return ApiResponse<string>.SuccessResponse(
                "If an account with that email exists, a password reset link has been sent.",
                null
            );

        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString(),
            ExpiryDate = DateTime.UtcNow.AddHours(1),
            TokenType = EmailTokenType.PasswordReset
        };

        await _tokenRepo.Add(token);

        var link = string.Format(PASSWORD_RESET_ROUTE, token.Token);

        await _emailService.SendEmailAsync(
            user.Email,
            "Reset your password",
            $"<p>Click <a href='{link}'>here</a> to reset your password.</p>"
        );

        return ApiResponse<string>.SuccessResponse(
            "If an account with that email exists, a password reset link has been sent.",
            null
        );
    }

    public async Task<ApiResponse<string>> ResetPassword(ResetPasswordDto dto)
    {
        var record = await _tokenRepo.GetByToken(dto.Token);

        if (record == null || record.TokenType != EmailTokenType.PasswordReset)
            return ApiResponse<string>.FailureResponse("Invalid or expired reset token");

        if (record.ExpiryDate < DateTime.UtcNow)
            return ApiResponse<string>.FailureResponse("Reset token expired");

        var user = await _userRepo.GetById(record.UserId);
        if (user == null)
            return ApiResponse<string>.FailureResponse("User not found");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepo.Update(user);
        await _tokenRepo.Delete(record);

        return ApiResponse<string>.SuccessResponse(
            "Password reset successfully.",
            null
        );
    }
}
