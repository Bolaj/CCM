using CityChoir.Domain.Enums;

namespace CityChoir.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public required string  FullName { get; set; }
    public required string Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Lga { get; set; }
    public required string Gender { get; set; }

    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; }
    public ChoirPart Part { get; set; }
    public Occupation Occupation { get; set; }
    public bool IsEmailVerified { get; set; }

    public DateTime CreatedAt { get; set; }
}