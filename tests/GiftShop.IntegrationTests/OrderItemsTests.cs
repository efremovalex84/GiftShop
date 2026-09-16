using System.Net.Http.Json;
using FluentAssertions;
using GiftShop.Api.Dtos;
using GiftShop.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GiftShop.IntegrationTests;

public class OrderItemsTests : IntegrationTestBase
{
    [Fact]
    public async Task Add_item_creates_order_item_and_recomputes_total()
    {
        var customer = await AddCustomerAsync();
        var coffee = await AddProductAsync(price: 500m, stock: 10, name: "Coffee");
        var tea = await AddProductAsync(price: 350m, stock: 10, name: "Tea");
        var order = await PlaceOrderAsync(customer.Id, coffee.Id, 1);

        var response = await Client.PostAsJsonAsync(
            $"/api/orders/{order.Id}/items",
            new AddItemRequest(tea.Id, 2));

        response.EnsureSuccessStatusCode();

        var items = await QueryDbAsync(db => db.OrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync());
        items.Should().HaveCount(2);

        var saved = await QueryDbAsync(db => db.Orders.SingleAsync(o => o.Id == order.Id));
        saved.Total.Should().Be(500m + 700m);
    }

    [Fact]
    public async Task Remove_item_deletes_it_and_recomputes_total()
    {
        var customer = await AddCustomerAsync();
        var coffee = await AddProductAsync(price: 500m, stock: 10, name: "Coffee");
        var tea = await AddProductAsync(price: 350m, stock: 10, name: "Tea");

        var request = new CreateOrderRequest(
            customer.Id,
            new List<CreateOrderItemDto> { new(coffee.Id, 1), new(tea.Id, 2) },
            new DeliveryDto("Nova Poshta", "Kyiv", "#15"));
        var created = await Client.PostAsJsonAsync("/api/orders", request);
        var order = (await created.Content.ReadFromJsonAsync<OrderResponse>())!;

        var teaItem = order.Items.Single(i => i.ProductId == tea.Id);

        var response = await Client.DeleteAsync($"/api/orders/{order.Id}/items/{teaItem.Id}");
        response.EnsureSuccessStatusCode();

        var items = await QueryDbAsync(db => db.OrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync());
        items.Should().ContainSingle();
        items.Single().ProductId.Should().Be(coffee.Id);

        var saved = await QueryDbAsync(db => db.Orders.SingleAsync(o => o.Id == order.Id));
        saved.Total.Should().Be(500m);
    }
}
