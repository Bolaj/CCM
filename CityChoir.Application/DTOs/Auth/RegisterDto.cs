using CityChoir.Domain.Enums;

namespace CityChoir.Application.DTOs.Auth;

public class RegisterDto
{
    public required string FullName { get; set; }
    public required string Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required string Lga { get; set; }
    public required string Gender { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public Occupation Occupation { get; set; }
    public ChoirPart Part { get; set; }
}