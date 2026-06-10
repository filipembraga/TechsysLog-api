using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;

namespace TechsysLog.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateAsync(CreateOrderDto dto, string userId);
    Task<OrderResponseDto?> GetByIdAsync(string orderId);
    Task<OrderResponseDto?> GetByOrderNumberAsync(string orderNumber); 
    Task<List<OrderResponseDto>> GetAllByUserIdAsync(string userId);
}