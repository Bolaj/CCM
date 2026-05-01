using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;
using CityChoir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CityChoir.Infrastructure.Repository;

public class EmailTokenRepository: IEmailTokenRepository
{
    private readonly AppDbContext _dbContext;

    public EmailTokenRepository(AppDbContext context)
    {
        _dbContext = context;
    }

    public async Task Add(EmailVerificationToken token)
    {
        await _dbContext.EmailVerificationTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<EmailVerificationToken> GetByToken(string token)
    {
        return await _dbContext.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task Delete(EmailVerificationToken token)
    {
        _dbContext.EmailVerificationTokens.Remove(token);
        await _dbContext.SaveChangesAsync();
    }
}