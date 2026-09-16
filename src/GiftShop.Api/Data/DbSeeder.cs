using GiftShop.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace GiftShop.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync())
            return;

        db.Products.AddRange(
            new Product { Name = "Coffee Gift Box", Price = 500m, Stock = 100 },
            new Product { Name = "Tea Sampler", Price = 350m, Stock = 100 },
            new Product { Name = "Chocolate Hamper", Price = 750m, Stock = 50 },
            new Product { Name = "Scented Candle Set", Price = 420m, Stock = 80 });

        db.Customers.AddRange(
            new Customer { Name = "Ivan", Phone = "+380000000001" },
            new Customer { Name = "Olena", Phone = "+380000000002" });

        await db.SaveChangesAsync();
    }
}
