using CityChoir.Domain.Entities;

namespace CityChoir.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByEmail(string email);
    Task Add(User user);
    Task<User> GetByEmail(string email);
    Task<User> GetById(Guid id);
    Task Update(User user);
    Task<IEnumerable<User>> GetActiveMembers();
}