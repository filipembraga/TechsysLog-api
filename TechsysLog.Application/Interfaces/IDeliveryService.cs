using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;

namespace TechsysLog.Application.Interfaces;

public interface IDeliveryService
{
    Task<DeliveryResponseDto> RegisterAsync(CreateDeliveryDto dto);
    Task<DeliveryResponseDto?> GetByOrderIdAsync(string orderId);
}