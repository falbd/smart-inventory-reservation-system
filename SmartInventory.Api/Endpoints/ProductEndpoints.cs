using Microsoft.EntityFrameworkCore;
using SmartInventory.Contracts.Products;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Entities;

namespace SmartInventory.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");
        
        // Add New Products
        group.MapPost("/", async (
            CreateProductRequest request,
            SmartInventoryDbContext db) =>
        {
            if (request.InitialStock < 0)
                return Results.BadRequest("Initial stock cannot be negative.");

            var skuExists = await db.Products.AnyAsync(x => x.Sku == request.Sku);

            if (skuExists)
                return Results.Conflict("Product SKU already exists.");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Sku = request.Sku,
                Price = request.Price,
                CreatedAtUtc = DateTime.UtcNow
            };

            var stockItem = new StockItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                QuantityAvailable = request.InitialStock,
                QuantityReserved = 0
            };

            db.Products.Add(product);
            db.StockItems.Add(stockItem);

            await db.SaveChangesAsync();

            var response = new ProductResponse(
                product.Id,
                product.Name,
                product.Sku,
                product.Price,
                stockItem.QuantityAvailable,
                stockItem.QuantityReserved
            );

            return Results.Created($"/api/products/{product.Id}", response);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));


        // Show All Products
        group.MapGet("/", async (SmartInventoryDbContext db) =>
        {
            var products = await db.Products
                .Join(
                    db.StockItems,
                    product => product.Id,
                    stock => stock.ProductId,
                    (product, stock) => new ProductResponse(
                        product.Id,
                        product.Name,
                        product.Sku,
                        product.Price,
                        stock.QuantityAvailable,
                        stock.QuantityReserved
                    ))
                .ToListAsync();

            return Results.Ok(products);
        })
         .RequireAuthorization(policy => policy.RequireRole("Admin", "Staff"));


        // Show All Products By Id
        group.MapGet("/{id:guid}", async (Guid id, SmartInventoryDbContext db) =>
        {
            var product = await db.Products
                .Where(p => p.Id == id)
                .Join(
                    db.StockItems,
                    product => product.Id,
                    stock => stock.ProductId,
                    (product, stock) => new ProductResponse(
                        product.Id,
                        product.Name,
                        product.Sku,
                        product.Price,
                        stock.QuantityAvailable,
                        stock.QuantityReserved
                    ))
                .FirstOrDefaultAsync();

            return product is null
                ? Results.NotFound()
                : Results.Ok(product);
        })
         .RequireAuthorization(policy => policy.RequireRole("Admin", "Staff"));

        return app;
    }
}
