using GiftShop.Api.Data;
using GiftShop.Api.Domain;
using GiftShop.Api.Dtos;
using GiftShop.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GiftShop.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db) => _db = db;

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await LoadOrderQuery()
                .FirstOrDefaultAsync(o => o.IdempotencyKey == request.IdempotencyKey);
            if (existing is not null)
                return Map(existing);
        }

        if (request.Items is null || request.Items.Count == 0)
            throw new BusinessRuleException("An order must contain at least one item.");

        var customer = await _db.Customers.FindAsync(request.CustomerId)
            ?? throw new NotFoundException($"Customer {request.CustomerId} does not exist.");

        // A single SaveChanges wraps all inserts and stock updates in one transaction,
        // so any validation failure below leaves the database unchanged.
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var order = new Order
        {
            CustomerId = customer.Id,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            IdempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey) ? null : request.IdempotencyKey
        };

        foreach (var line in request.Items)
        {
            if (line.Quantity <= 0)
                throw new BusinessRuleException("Item quantity must be greater than zero.");

            var product = await _db.Products.FindAsync(line.ProductId)
                ?? throw new NotFoundException($"Product {line.ProductId} does not exist.");

            if (product.Stock < line.Quantity)
                throw new BusinessRuleException($"Insufficient stock for product {product.Id}.");

            product.Stock -= line.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = line.Quantity,
                UnitPrice = product.Price
            });
        }

        order.RecalculateTotal();
        order.Delivery = new Delivery
        {
            Method = request.Delivery.Method,
            Address = request.Delivery.Address,
            Branch = request.Delivery.Branch
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Map(await RequireOrderAsync(order.Id));
    }

    public async Task<OrderResponse> GetOrderAsync(int orderId)
        => Map(await RequireOrderAsync(orderId));

    public async Task<OrderResponse> ChangeStatusAsync(int orderId, string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
            throw new BusinessRuleException($"Unknown status '{status}'.");

        var order = await RequireOrderAsync(orderId);
        order.Status = parsed;
        await _db.SaveChangesAsync();
        return Map(order);
    }

    public async Task<OrderResponse> CancelOrderAsync(int orderId)
    {
        var order = await RequireOrderAsync(orderId);
        if (order.Status == OrderStatus.Cancelled)
            return Map(order);

        foreach (var item in order.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product is not null)
                product.Stock += item.Quantity;
        }

        order.Status = OrderStatus.Cancelled;
        await _db.SaveChangesAsync();
        return Map(order);
    }

    public async Task<OrderResponse> UpdateDeliveryAsync(int orderId, UpdateDeliveryRequest request)
    {
        var order = await RequireOrderAsync(orderId);
        order.Delivery ??= new Delivery { OrderId = order.Id };
        order.Delivery.Method = request.Method;
        order.Delivery.Address = request.Address;
        order.Delivery.Branch = request.Branch;
        await _db.SaveChangesAsync();
        return Map(order);
    }

    public async Task<OrderResponse> AddItemAsync(int orderId, AddItemRequest request)
    {
        if (request.Quantity <= 0)
            throw new BusinessRuleException("Item quantity must be greater than zero.");

        var order = await RequireOrderAsync(orderId);
        var product = await _db.Products.FindAsync(request.ProductId)
            ?? throw new NotFoundException($"Product {request.ProductId} does not exist.");

        if (product.Stock < request.Quantity)
            throw new BusinessRuleException($"Insufficient stock for product {product.Id}.");

        product.Stock -= request.Quantity;
        order.Items.Add(new OrderItem
        {
            ProductId = product.Id,
            Quantity = request.Quantity,
            UnitPrice = product.Price
        });
        order.RecalculateTotal();
        await _db.SaveChangesAsync();
        return Map(order);
    }

    public async Task<OrderResponse> RemoveItemAsync(int orderId, int itemId)
    {
        var order = await RequireOrderAsync(orderId);
        var item = order.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException($"Order item {itemId} does not exist on order {orderId}.");

        var product = await _db.Products.FindAsync(item.ProductId);
        if (product is not null)
            product.Stock += item.Quantity;

        order.Items.Remove(item);
        _db.OrderItems.Remove(item);
        order.RecalculateTotal();
        await _db.SaveChangesAsync();
        return Map(order);
    }

    private IQueryable<Order> LoadOrderQuery()
        => _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Delivery);

    private async Task<Order> RequireOrderAsync(int orderId)
        => await LoadOrderQuery().FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException($"Order {orderId} does not exist.");

    private static OrderResponse Map(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.Total,
        order.CreatedAt,
        order.Items
            .OrderBy(i => i.Id)
            .Select(i => new OrderItemResponse(i.Id, i.ProductId, i.Quantity, i.UnitPrice))
            .ToList(),
        order.Delivery is null
            ? null
            : new DeliveryResponse(order.Delivery.Method, order.Delivery.Address, order.Delivery.Branch));
}
