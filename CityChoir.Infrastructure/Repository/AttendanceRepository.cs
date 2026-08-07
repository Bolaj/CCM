// CityChoir.Infrastructure/Repository/AttendanceRepository.cs
using CityChoir.Application.DTOs.Attendance;
using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;
using CityChoir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CityChoir.Infrastructure.Repository;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;

    public AttendanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Attendance>> GetAll(AttendanceFilterDto filter)
    {
        var query = _context.Attendances
            .Include(a => a.User)
            .Include(a => a.Rehearsal)
            .AsQueryable();

        if (filter.RehearsalId.HasValue)
            query = query.Where(a => a.RehearsalId == filter.RehearsalId.Value);

        if (filter.From.HasValue)
            query = query.Where(a => a.MarkedAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(a => a.MarkedAt <= filter.To.Value);

        if (filter.Part.HasValue)
            query = query.Where(a => a.User.Part == filter.Part.Value);

        if (!string.IsNullOrEmpty(filter.Gender))
            query = query.Where(a => a.User.Gender == filter.Gender);

        return await query.ToListAsync();
    }

    public async Task<Attendance?> GetByUserAndRehearsal(Guid userId, int rehearsalId)
    {
        return await _context.Attendances
            .FirstOrDefaultAsync(a => a.UserId == userId && a.RehearsalId == rehearsalId);
    }

    public async Task<int> GetTotalRehearsalsCount(AttendanceFilterDto filter)
    {
        var query = _context.Rehearsals.AsQueryable();

        if (filter.From.HasValue)
            query = query.Where(r => r.RehearsalDate >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(r => r.RehearsalDate <= filter.To.Value);

        if (filter.RehearsalId.HasValue)
            query = query.Where(r => r.Id == filter.RehearsalId.Value);

        return await query.CountAsync();
    }

    public async Task Add(Attendance attendance)
    {
        await _context.Attendances.AddAsync(attendance);
        await _context.SaveChangesAsync();
    }
    
}