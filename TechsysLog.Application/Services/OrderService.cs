using Microsoft.Extensions.Logging;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Services;

/// <summary>
/// Handles order creation and retrieval.
///
/// Design decision: OrderNumber is generated sequentially in the format ORD-00001.
/// GUID was considered but rejected for operational reasons — order numbers are
/// communicated between customers and support teams and must be human-readable.
/// Sequential numbers are safe here because routes are JWT-protected and
/// filtered by UserId, preventing enumeration attacks.
///
/// Design decision: address is enriched via ViaCEP before persistence.
/// The user provides only the ZipCode — street, neighborhood, city and state
/// are fetched automatically. The user-provided Number field is preserved.
/// If ViaCEP is unavailable, the order is still created with the data provided.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IAddressLookupService _addressLookupService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IAddressLookupService addressLookupService,
        INotificationService notificationService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _addressLookupService = addressLookupService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto, string userId)
    {
        var enrichedAddress = await EnrichAddressAsync(dto.DeliveryAddress);

        var count = await _orderRepository.CountAsync();
        var orderNumber = $"ORD-{(count + 1):D5}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            Description = dto.Description,
            Amount = dto.Amount,
            DeliveryAddress = enrichedAddress,
            Status = OrderStatus.Pending,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepository.CreateAsync(order);

        await _notificationService.SendAsync(
            $"New order {orderNumber} registered.",
            order.Id);

        return MapToResponse(order);
    }

    public async Task<OrderResponseDto?> GetByIdAsync(string id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<OrderResponseDto?> GetByOrderNumberAsync(string orderNumber)
    {
        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<List<OrderResponseDto>> GetAllByUserIdAsync(string userId)
    {
        var orders = await _orderRepository.GetAllByUserIdAsync(userId);
        return orders.Select(MapToResponse).ToList();
    }

    /// <summary>
    /// Attempts to enrich the address using ViaCEP.
    /// If the external service fails, falls back to the data provided by the user.
    /// </summary>
    private async Task<Address> EnrichAddressAsync(AddressDto dto)
    {
        try
        {
            var viaCepAddress = await _addressLookupService.GetAddressByZipCodeAsync(dto.ZipCode);

            if (viaCepAddress is not null)
            {
                return new Address(
                    zipCode: dto.ZipCode,
                    street: viaCepAddress.Street,
                    number: dto.Number,
                    neighborhood: viaCepAddress.Neighborhood,
                    city: viaCepAddress.City,
                    state: viaCepAddress.State);
            }
        }
        catch (Exception ex)
        {
            // This is a non-critical failure: address enrichment is best-effort.
            _logger.LogWarning(ex,
                "Address lookup failed for zip code {ZipCode}. " +
                "Falling back to user-provided address data.", dto.ZipCode); // ViaCEP unavailable — fallback to user-provided data
        }

        return new Address(
            zipCode: dto.ZipCode,
            street: dto.Street,
            number: dto.Number,
            neighborhood: dto.Neighborhood,
            city: dto.City,
            state: dto.State);
    }

    private static OrderResponseDto MapToResponse(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        Description = order.Description,
        Amount = order.Amount,
        Status = order.Status,
        UserId = order.UserId,
        CreatedAt = order.CreatedAt,
        DeliveryAddress = new AddressDto
        {
            ZipCode = order.DeliveryAddress.ZipCode,
            Street = order.DeliveryAddress.Street,
            Number = order.DeliveryAddress.Number,
            Neighborhood = order.DeliveryAddress.Neighborhood,
            City = order.DeliveryAddress.City,
            State = order.DeliveryAddress.State
        }
    };
}