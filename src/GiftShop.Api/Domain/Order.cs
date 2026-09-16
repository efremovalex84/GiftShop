namespace GiftShop.Api.Domain;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }

    // Optional client-supplied key used to guarantee idempotent order creation.
    public string? IdempotencyKey { get; set; }

    public List<OrderItem> Items { get; set; } = new();
    public Delivery? Delivery { get; set; }

    public void RecalculateTotal() => Total = Items.Sum(i => i.UnitPrice * i.Quantity);
}
