using GiftShop.Api.Dtos;

namespace GiftShop.Api.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse> GetOrderAsync(int orderId);
    Task<OrderResponse> ChangeStatusAsync(int orderId, string status);
    Task<OrderResponse> CancelOrderAsync(int orderId);
    Task<OrderResponse> UpdateDeliveryAsync(int orderId, UpdateDeliveryRequest request);
    Task<OrderResponse> AddItemAsync(int orderId, AddItemRequest request);
    Task<OrderResponse> RemoveItemAsync(int orderId, int itemId);
}
