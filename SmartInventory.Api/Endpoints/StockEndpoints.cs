using Microsoft.EntityFrameworkCore;
using SmartInventory.Contracts.Stock;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Api.Endpoints;

public static class StockEndpoints
{
    public static IEndpointRouteBuilder MapStockEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stock")
            .WithTags("Stock");

        // Get All Stocks
        group.MapGet("/", async (SmartInventoryDbContext db) =>
        {
            var stock = await db.StockItems
                .Include(x => x.Product)
                .Select(x => new StockResponse(
                    x.ProductId,
                    x.Product.Name,
                    x.Product.Sku,
                    x.QuantityAvailable,
                    x.QuantityReserved,
                    x.QuantityAvailable + x.QuantityReserved,
                    x.UpdatedAtUtc
                ))
                .ToListAsync();

            return Results.Ok(stock);
        })
         .RequireAuthorization(policy => policy.RequireRole("Admin", "Staff"));
        
        // Get Stocks By ProductId
        group.MapGet("/{productId:guid}", async (Guid productId, SmartInventoryDbContext db) =>
        {
            var stock = await db.StockItems
                .Include(x => x.Product)
                .Where(x => x.ProductId == productId)
                .Select(x => new StockResponse(
                    x.ProductId,
                    x.Product.Name,
                    x.Product.Sku,
                    x.QuantityAvailable,
                    x.QuantityReserved,
                    x.QuantityAvailable + x.QuantityReserved,
                    x.UpdatedAtUtc
                ))
                .FirstOrDefaultAsync();

            return stock is null
                ? Results.NotFound()
                : Results.Ok(stock);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin", "Staff"));

        // Add New Stocks using ProductId
        group.MapPost("/{productId:guid}/add", async (
            Guid productId,
            AddStockRequest request,
            SmartInventoryDbContext db) =>
        {
            if (request.Quantity <= 0)
                return Results.BadRequest("Quantity must be greater than zero.");

            var stockItem = await db.StockItems
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            if (stockItem is null)
                return Results.NotFound("Stock item not found.");

            stockItem.QuantityAvailable += request.Quantity;
            stockItem.UpdatedAtUtc = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                stockItem.ProductId,
                stockItem.QuantityAvailable,
                stockItem.QuantityReserved
            });
        })
         .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}
