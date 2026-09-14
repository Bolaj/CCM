using CityChoir.Application.DTOs.Common;
using CityChoir.Application.DTOs.Rehearsal;
using CityChoir.Domain.Entities;

namespace CityChoir.Application.Interfaces;

public interface IRehearsalService
{
   Task<ApiResponse<IEnumerable<Rehearsal>>> GetAll();
    Task<ApiResponse<Rehearsal?>> GetById(int id);
    Task<ApiResponse<IEnumerable<UserRehearsalDto>>> GetUpcomingForUser(Guid userId);
    Task<ApiResponse<IEnumerable<UserRehearsalDto>>> GetHistoryForUser(Guid userId);
    Task<ApiResponse<string>> Create(CreateRehearsalDto rehearsalDto);
    Task<ApiResponse<string>> Update(UpdateRehearsalDto rehearsalDto);
    Task<ApiResponse<string>> Delete(int id);
    Task<bool> Exists(int id);
}
