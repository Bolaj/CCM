using CityChoir.Domain.Enums;

namespace CityChoir.Application.DTOs.Admin;

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string RegistrationNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string Gender { get; set; }
    public UserRole Role { get; set; }
    public ChoirPart Part { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}