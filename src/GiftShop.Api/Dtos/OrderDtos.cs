namespace GiftShop.Api.Dtos;

public record CreateOrderItemDto(int ProductId, int Quantity);

public record DeliveryDto(string Method, string Address, string Branch);

public record CreateOrderRequest(
    int CustomerId,
    List<CreateOrderItemDto> Items,
    DeliveryDto Delivery,
    string? IdempotencyKey = null);

public record UpdateStatusRequest(string Status);

public record UpdateDeliveryRequest(string Method, string Address, string Branch);

public record AddItemRequest(int ProductId, int Quantity);

public record OrderItemResponse(int Id, int ProductId, int Quantity, decimal UnitPrice);

public record DeliveryResponse(string Method, string Address, string Branch);

public record OrderResponse(
    int Id,
    int CustomerId,
    string Status,
    decimal Total,
    DateTime CreatedAt,
    List<OrderItemResponse> Items,
    DeliveryResponse? Delivery);
