using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GiftShop.Api.Dtos;
using GiftShop.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiftShop.IntegrationTests;

public class TransactionTests : IntegrationTestBase
{
    private static DeliveryDto Delivery => new("Nova Poshta", "Kyiv", "#15");

    [Fact]
    public async Task Insufficient_stock_rolls_back_the_whole_order()
    {
        var customer = await AddCustomerAsync();
        var inStock = await AddProductAsync(price: 500m, stock: 10, name: "Coffee");
        var lowStock = await AddProductAsync(price: 350m, stock: 1, name: "Tea");

        var request = new CreateOrderRequest(
            customer.Id,
            new List<CreateOrderItemDto> { new(inStock.Id, 2), new(lowStock.Id, 5) },
            Delivery);

        var response = await Client.PostAsJsonAsync("/api/orders", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var orderCount = await QueryDbAsync(db => db.Orders.CountAsync());
        orderCount.Should().Be(0);

        var itemCount = await QueryDbAsync(db => db.OrderItems.CountAsync());
        itemCount.Should().Be(0);

        // The in-stock product's stock must not have been decremented.
        var reloaded = await QueryDbAsync(db => db.Products.SingleAsync(p => p.Id == inStock.Id));
        reloaded.Stock.Should().Be(10);
    }

    [Fact]
    public async Task Duplicate_request_with_same_idempotency_key_creates_one_order()
    {
        var customer = await AddCustomerAsync();
        var product = await AddProductAsync(price: 500m, stock: 10);

        var request = new CreateOrderRequest(
            customer.Id,
            new List<CreateOrderItemDto> { new(product.Id, 1) },
            Delivery,
            IdempotencyKey: "order-123");

        var first = await Client.PostAsJsonAsync("/api/orders", request);
        var second = await Client.PostAsJsonAsync("/api/orders", request);

        var firstBody = await first.Content.ReadFromJsonAsync<OrderResponse>();
        var secondBody = await second.Content.ReadFromJsonAsync<OrderResponse>();
        secondBody!.Id.Should().Be(firstBody!.Id);

        var orderCount = await QueryDbAsync(db => db.Orders.CountAsync());
        orderCount.Should().Be(1);
    }
}
