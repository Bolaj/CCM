using CityChoir.Domain.Entities;

namespace CityChoir.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}