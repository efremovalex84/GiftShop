using System.Net.Http.Json;
using FluentAssertions;
using GiftShop.Api.Domain;
using GiftShop.Api.Dtos;
using GiftShop.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiftShop.IntegrationTests;

public class UpdateOrderTests : IntegrationTestBase
{
    [Fact]
    public async Task Change_status_updates_the_database()
    {
        var customer = await AddCustomerAsync();
        var product = await AddProductAsync(price: 500m, stock: 10);
        var order = await PlaceOrderAsync(customer.Id, product.Id, 1);

        var response = await Client.PatchAsJsonAsync(
            $"/api/orders/{order.Id}/status",
            new UpdateStatusRequest(nameof(OrderStatus.Shipped)));

        response.EnsureSuccessStatusCode();

        var saved = await QueryDbAsync(db => db.Orders.SingleAsync(o => o.Id == order.Id));
        saved.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public async Task Get_order_returns_data_matching_the_database()
    {
        var customer = await AddCustomerAsync();
        var product = await AddProductAsync(price: 350m, stock: 10);
        var order = await PlaceOrderAsync(customer.Id, product.Id, 3);

        var body = await Client.GetFromJsonAsync<OrderResponse>($"/api/orders/{order.Id}");

        var saved = await QueryDbAsync(db => db.Orders
            .Include(o => o.Items)
            .SingleAsync(o => o.Id == order.Id));

        body!.Id.Should().Be(saved.Id);
        body.CustomerId.Should().Be(saved.CustomerId);
        body.Total.Should().Be(saved.Total);
        body.Status.Should().Be(saved.Status.ToString());
        body.Items.Should().HaveCount(saved.Items.Count);
    }
}
