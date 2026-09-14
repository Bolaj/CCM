using CityChoir.Application.DTOs.Attendance;
using CityChoir.Domain.Entities;

namespace CityChoir.Application.Interfaces;

public interface IAttendanceRepository
{
    Task<IEnumerable<Attendance>> GetAll(AttendanceFilterDto filter);
    Task<Attendance?> GetByUserAndRehearsal(Guid userId, int rehearsalId);
    Task<IEnumerable<Attendance>> GetByUserId(Guid userId);
    Task<int> GetTotalRehearsalsCount(AttendanceFilterDto filter);

    Task Add(Attendance attendance);
}