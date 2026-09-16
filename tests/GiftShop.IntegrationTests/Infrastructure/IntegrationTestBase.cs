using GiftShop.Api.Data;
using GiftShop.Api.Domain;
using GiftShop.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace GiftShop.IntegrationTests.Infrastructure;

// Base class: fresh in-memory database per test, plus helpers to seed and
// to assert against the database through an independent DbContext scope.
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase()
    {
        Factory = new CustomWebApplicationFactory();

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        Client = Factory.CreateClient();
    }

    protected async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }

    protected async Task<Product> AddProductAsync(decimal price, int stock, string name = "Gift Box")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var product = new Product { Name = name, Price = price, Stock = stock };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    protected async Task<Customer> AddCustomerAsync(string name = "Ivan", string phone = "+380000000000")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = new Customer { Name = name, Phone = phone };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer;
    }

    // Places an order through the public API so tests operate on real persisted state.
    protected async Task<OrderResponse> PlaceOrderAsync(int customerId, int productId, int quantity)
    {
        var request = new CreateOrderRequest(
            customerId,
            new List<CreateOrderItemDto> { new(productId, quantity) },
            new DeliveryDto("Nova Poshta", "Kyiv", "#15"));

        var response = await Client.PostAsJsonAsync("/api/orders", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
