using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces;

public interface IDeliveryRepository
{
    Task CreateAsync(Delivery delivery);
    Task<Delivery?> GetByOrderIdAsync(string orderId);
    Task<bool> OrderAlreadyDeliveredAsync(string orderId);
}
