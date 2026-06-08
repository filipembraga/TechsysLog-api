namespace TechsysLog.Domain.Interfaces
{
    using System.Threading.Tasks;
    using TechsysLog.Domain.Entities;
    using TechsysLog.Domain.Enums;
    public interface IOrderRepository
    {
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<List<Order>> GetByUserIdAsync(string userId);
        Task UpdateStatusAsync(string id, OrderStatus status);
    }
}