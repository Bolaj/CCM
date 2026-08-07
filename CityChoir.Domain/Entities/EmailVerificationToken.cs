namespace CityChoir.Domain.Entities;

using CityChoir.Domain.Enums;

public class EmailVerificationToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public required string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public EmailTokenType TokenType { get; set; }
}