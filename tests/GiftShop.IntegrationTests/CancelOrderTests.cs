using FluentAssertions;
using GiftShop.Api.Domain;
using GiftShop.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiftShop.IntegrationTests;

public class CancelOrderTests : IntegrationTestBase
{
    [Fact]
    public async Task Cancel_order_sets_status_and_restores_stock()
    {
        var customer = await AddCustomerAsync();
        var product = await AddProductAsync(price: 500m, stock: 10);
        var order = await PlaceOrderAsync(customer.Id, product.Id, 3);

        var afterOrder = await QueryDbAsync(db => db.Products.SingleAsync(p => p.Id == product.Id));
        afterOrder.Stock.Should().Be(7);

        var response = await Client.PostAsync($"/api/orders/{order.Id}/cancel", null);
        response.EnsureSuccessStatusCode();

        var saved = await QueryDbAsync(db => db.Orders.SingleAsync(o => o.Id == order.Id));
        saved.Status.Should().Be(OrderStatus.Cancelled);

        var restored = await QueryDbAsync(db => db.Products.SingleAsync(p => p.Id == product.Id));
        restored.Stock.Should().Be(10);
    }
}
