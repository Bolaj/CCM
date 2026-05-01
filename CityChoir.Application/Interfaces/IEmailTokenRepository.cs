using CityChoir.Domain.Entities;

namespace CityChoir.Application.Interfaces;

public interface IEmailTokenRepository
{
    Task Add(EmailVerificationToken token);
    Task<EmailVerificationToken> GetByToken(string token);
    Task Delete(EmailVerificationToken token);
}