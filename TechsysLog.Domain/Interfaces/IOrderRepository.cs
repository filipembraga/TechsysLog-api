using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Domain.Interfaces;

public interface IOrderRepository
{
    Task CreateAsync(Order order);
    Task<Order?> GetByIdAsync(string id);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<List<Order>> GetAllByUserIdAsync(string userId);
    Task UpdateStatusAsync(string id, OrderStatus status);
    Task<long> CountAsync();
}