namespace GiftShop.Api.Exceptions;

// Thrown when a requested entity does not exist. Mapped to HTTP 404.
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
