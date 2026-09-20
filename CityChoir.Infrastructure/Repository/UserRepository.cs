using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;
using CityChoir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CityChoir.Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    public UserRepository(AppDbContext context)
    {
        _dbContext = context;
    }
    public async Task<bool> ExistsByEmail(string email)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant();
        return await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task Add(User user)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        var year = DateTime.UtcNow.Year;
        var part = user.Part.ToString().ToUpperInvariant();

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO RegistrationSequences (Year, Part, NextNumber)
            VALUES ({year}, {part}, 1)
            ON DUPLICATE KEY UPDATE NextNumber = NextNumber + 1
            """);

        var sequence = await _dbContext.RegistrationSequences
            .SingleAsync(value => value.Year == year && value.Part == part);

        user.RegistrationNumber = $"{part}/{year}/{sequence.NextNumber:D3}";

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<User> GetByEmail(string email)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant();
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task<User> GetById(Guid id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task Update(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(User user)
    {
        var tokens = await _dbContext.EmailVerificationTokens
            .Where(token => token.UserId == user.Id)
            .ToListAsync();

        _dbContext.EmailVerificationTokens.RemoveRange(tokens);
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<User>> GetActiveMembers()
    {
        return await _dbContext.Users
            .Where(u => u.IsEmailVerified)
            .ToListAsync();
    }
    
}