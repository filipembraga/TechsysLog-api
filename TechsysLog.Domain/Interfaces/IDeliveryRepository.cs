namespace TechsysLog.Domain.Interfaces
{
    using System.Threading.Tasks;
    using TechsysLog.Domain.Entities;

    public interface IDeliveryRepository
    {
        Task<Delivery?> GetByOrderIdAsync(string orderId);
        Task<bool> OrderAlreadyDeliveredAsync(string orderId);
    }
}