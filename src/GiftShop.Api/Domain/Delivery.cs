namespace GiftShop.Api.Domain;

public class Delivery
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public string Method { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
}
