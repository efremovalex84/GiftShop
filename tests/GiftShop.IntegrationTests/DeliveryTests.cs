using System.Net.Http.Json;
using FluentAssertions;
using GiftShop.Api.Dtos;
using GiftShop.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiftShop.IntegrationTests;

public class DeliveryTests : IntegrationTestBase
{
    [Fact]
    public async Task Update_delivery_changes_only_the_delivery_row()
    {
        var customer = await AddCustomerAsync();
        var product = await AddProductAsync(price: 500m, stock: 10);
        var order = await PlaceOrderAsync(customer.Id, product.Id, 2);

        var response = await Client.PutAsJsonAsync(
            $"/api/orders/{order.Id}/delivery",
            new UpdateDeliveryRequest("Ukrposhta", "Lviv", "#42"));

        response.EnsureSuccessStatusCode();

        var savedOrder = await QueryDbAsync(db => db.Orders.SingleAsync(o => o.Id == order.Id));
        var savedDelivery = await QueryDbAsync(db => db.Deliveries.SingleAsync(d => d.OrderId == order.Id));

        savedDelivery.Method.Should().Be("Ukrposhta");
        savedDelivery.Address.Should().Be("Lviv");
        savedDelivery.Branch.Should().Be("#42");

        // Order totals and status are untouched by a delivery change.
        savedOrder.Total.Should().Be(order.Total);
        savedOrder.Status.ToString().Should().Be(order.Status);
    }
}
