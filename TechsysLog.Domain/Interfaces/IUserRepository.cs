using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
    }  
}

 