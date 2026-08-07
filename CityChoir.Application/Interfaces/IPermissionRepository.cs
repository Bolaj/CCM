using CityChoir.Domain.Entities;

namespace CityChoir.Application.Interfaces;

public interface IPermissionRepository
{
    Task<IEnumerable<Permission>> GetAll();
    Task<Permission?> GetById(Guid id);
    Task Add(Permission permission);
    Task Update(Permission permission);
}