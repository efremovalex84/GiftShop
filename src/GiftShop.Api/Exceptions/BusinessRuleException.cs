namespace GiftShop.Api.Exceptions;

// Thrown when a business rule is violated (e.g. insufficient stock). Mapped to HTTP 400.
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
