using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces;

public interface IUserRepository
{
    Task CreateAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(string id);
}