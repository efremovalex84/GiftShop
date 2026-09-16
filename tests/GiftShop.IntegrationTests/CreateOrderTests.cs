using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GiftShop.Api.Domain;
using GiftShop.Api.Dtos;
using GiftShop.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiftShop.IntegrationTests;

public class CreateOrderTests : IntegrationTestBase
{
    private static DeliveryDto Delivery => new("Nova Poshta", "Kyiv", "#15");

    [Fact]
    public async Task Create_order_persists_order_items_and_delivery()
    {
        var customer = await AddCustomerAsync();
        var product = await AddProductAsync(price: 500m, stock: 10);

        var request = new CreateOrderRequest(
            customer.Id,
            new List<CreateOrderItemDto> { new(product.Id, 2) },
            Delivery);

        var response = await Client.PostAsJsonAsync("/api/orders", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Total.Should().Be(1000m);
        body.Status.Should().Be(nameof(OrderStatus.Pending));

        var saved = await QueryDbAsync(db => db.Orders
            .Include(o => o.Items)
            .Include(o => o.Delivery)
            .SingleAsync(o => o.Id == body.Id));

        saved.CustomerId.Should().Be(customer.Id);
        saved.Status.Should().Be(OrderStatus.Pending);
        saved.Total.Should().Be(1000m);
        saved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        saved.Items.Should().ContainSingle();
        var item = saved.Items.Single();
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(500m);

        saved.Delivery.Should().NotBeNull();
        saved.Delivery!.OrderId.Should().Be(saved.Id);
        saved.Delivery.Method.Should().Be("Nova Poshta");
    }

    [Fact]
    public async Task Create_order_with_multiple_products_persists_each_item()
    {
        var customer = await AddCustomerAsync();
        var coffee = await AddProductAsync(price: 500m, stock: 10, name: "Coffee");
        var tea = await AddProductAsync(price: 350m, stock: 10, name: "Tea");

        var request = new CreateOrderRequest(
            customer.Id,
            new List<CreateOrderItemDto> { new(coffee.Id, 2), new(tea.Id, 1) },
            Delivery);

        var response = await Client.PostAsJsonAsync("/api/orders", request);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();

        var items = await QueryDbAsync(db => db.OrderItems
            .Where(i => i.OrderId == body!.Id)
            .ToListAsync());

        items.Should().HaveCount(2);
        items.Sum(i => i.UnitPrice * i.Quantity).Should().Be(1350m);
        body!.Total.Should().Be(1350m);
    }

    [Fact]
    public async Task Create_order_with_invalid_product_saves_nothing()
    {
        var customer = await AddCustomerAsync();

        var request = new CreateOrderRequest(
            customer.Id,
            new List<CreateOrderItemDto> { new(9999, 1) },
            Delivery);

        var response = await Client.PostAsJsonAsync("/api/orders", request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var orderCount = await QueryDbAsync(db => db.Orders.CountAsync());
        orderCount.Should().Be(0);
    }

    [Fact]
    public async Task Create_order_for_missing_customer_saves_nothing()
    {
        var product = await AddProductAsync(price: 500m, stock: 10);

        var request = new CreateOrderRequest(
            CustomerId: 9999,
            new List<CreateOrderItemDto> { new(product.Id, 1) },
            Delivery);

        var response = await Client.PostAsJsonAsync("/api/orders", request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var orderCount = await QueryDbAsync(db => db.Orders.CountAsync());
        orderCount.Should().Be(0);

        var reloaded = await QueryDbAsync(db => db.Products.SingleAsync(p => p.Id == product.Id));
        reloaded.Stock.Should().Be(10);
    }
}
