using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;
using CityChoir.Domain.Enums;
using CityChoir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CityChoir.Infrastructure.Repository;

public class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _dbContext;
    public PermissionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<Permission>> GetAll()
    {
        return await _dbContext.Permissions
            .Include(p => p.User)
            .Include(p => p.Rehearsal)
            .OrderByDescending(p => p.RequestedAt)
            .ToListAsync();
    }
    public async Task<Permission?> GetById(Guid id)
    {
        return await _dbContext.Permissions
            .Include(p => p.User)
            .Include(p => p.Rehearsal)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task Add(Permission permission)
    {
        await _dbContext.Permissions.AddAsync(permission);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(Permission permission)
    {
        _dbContext.Permissions.Update(permission);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> ApproveIfPending(Permission permission)
    {
        var updatedRows = await _dbContext.Permissions
            .Where(p => p.Id == permission.Id && p.Status == PermissionStatus.PENDING)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Status, PermissionStatus.APPROVED)
                .SetProperty(p => p.ReviewedAt, permission.ReviewedAt));

        return updatedRows == 1;
    }
    
}