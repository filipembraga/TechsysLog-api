using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Services;

/// <summary>
/// Handles delivery registration.
///
/// Design decision: registering a delivery automatically updates
/// the related order status to Delivered. 
///
/// Design decision: duplicate deliveries are blocked at the service level.
/// An order can only be delivered once. 
/// </summary>
public class DeliveryService : IDeliveryService
{
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly INotificationService _notificationService;

    public DeliveryService(
        IDeliveryRepository deliveryRepository,
        IOrderRepository orderRepository,
        INotificationService notificationService)
    {
        _deliveryRepository = deliveryRepository;
        _orderRepository = orderRepository;
        _notificationService = notificationService;
    }

    public async Task<DeliveryResponseDto> RegisterAsync(CreateDeliveryDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(dto.OrderId);

        if (order is null)
            throw new KeyNotFoundException($"Order {dto.OrderId} not found.");

        if (order.Status == OrderStatus.Delivered)
            throw new InvalidOperationException($"Order {order.OrderNumber} has already been delivered.");

        var alreadyDelivered = await _deliveryRepository.OrderAlreadyDeliveredAsync(dto.OrderId);

        if (alreadyDelivered)
            throw new InvalidOperationException($"A delivery for order {order.OrderNumber} already exists.");

        var delivery = new Delivery
        {
            OrderId = dto.OrderId,
            DeliveryDate = dto.DeliveredAt ?? DateTime.UtcNow
        };

        await _deliveryRepository.CreateAsync(delivery);

        await _orderRepository.UpdateStatusAsync(dto.OrderId, OrderStatus.Delivered);

        await _notificationService.SendAsync(
            $"Order {order.OrderNumber} has been delivered.",
            order.Id,
            NotificationType.OrderDelivered);

        return MapToResponse(delivery);
    }

    public async Task<DeliveryResponseDto?> GetByOrderIdAsync(string orderId)
    {
        var delivery = await _deliveryRepository.GetByOrderIdAsync(orderId);
        
        return delivery is null ? null : MapToResponse(delivery);
    }

    private static DeliveryResponseDto MapToResponse(Delivery delivery) => new()
    {
        Id = delivery.Id,
        OrderId = delivery.OrderId,
        DeliveredAt = delivery.DeliveryDate
    };
}