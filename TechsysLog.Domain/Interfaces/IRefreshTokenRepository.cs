using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken token);
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task DeleteByHashAsync(string tokenHash);
}