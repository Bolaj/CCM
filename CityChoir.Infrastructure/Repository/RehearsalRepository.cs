using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;
using CityChoir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CityChoir.Infrastructure.Repository;

public class RehearsalRepository : IRehearsalRepository
{
    private readonly AppDbContext _dbContext;

    public RehearsalRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Rehearsal>> GetAll()
    {
        return await _dbContext.Rehearsals.ToListAsync();
    }

    public async Task<Rehearsal?> GetById(int id)
    {
        return await _dbContext.Rehearsals.FindAsync(id);
    }

    public async Task Add(Rehearsal rehearsal)
    {
        await _dbContext.Rehearsals.AddAsync(rehearsal);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(Rehearsal rehearsal)
    {
        _dbContext.Rehearsals.Update(rehearsal);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var rehearsal = await _dbContext.Rehearsals.FindAsync(id);
        if (rehearsal is null)
            return;

        _dbContext.Rehearsals.Remove(rehearsal);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
        return await _dbContext.Rehearsals.AnyAsync(r => r.Id == id);
    }
}
