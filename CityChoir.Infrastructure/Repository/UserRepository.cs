using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;
using CityChoir.Infrastructure.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
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
    
    public async Task<IEnumerable<User>> GetActiveMembers()
    {
        return await _dbContext.Users
            .Where(u => u.IsEmailVerified)
            .ToListAsync();
    }
    
}