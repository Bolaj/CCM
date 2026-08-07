using CityChoir.Domain.Entities;

namespace CityChoir.Application.Interfaces;

public interface IRehearsalRepository
{
    Task<IEnumerable<Rehearsal>> GetAll();
    Task<Rehearsal?> GetById(int id);
    Task Add(Rehearsal rehearsal);
    Task Update(Rehearsal rehearsal);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
