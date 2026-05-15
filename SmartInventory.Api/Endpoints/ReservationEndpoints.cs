using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Api.Hubs;
using SmartInventory.Contracts.Reservations;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Entities;
using SmartInventory.Infrastructure.Services;

namespace SmartInventory.Api.Endpoints;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reservations")
            .WithTags("Reservations");

        // Add New Reservations
        group.MapPost("/", async (CreateReservationRequest request, SmartInventoryDbContext db,
            RedisLockService lockService , IHubContext<InventoryHub> hub) =>
        {
            if (request.Quantity <= 0)
                return Results.BadRequest("Quantity must be greater than zero.");

            var lockKey = $"lock:product:{request.ProductId}";
            var lockValue = Guid.NewGuid().ToString();

            var acquired = await lockService.AcquireLockAsync(
                lockKey,
                lockValue,
                TimeSpan.FromSeconds(10));

            if (!acquired)
                return Results.Conflict("Another reservation is currently being processed. Please try again.");

            try
            {
                var productExists = await db.Products
                    .AnyAsync(x => x.Id == request.ProductId);

                if (!productExists)
                    return Results.NotFound("Product not found.");

                var stockItem = await db.StockItems
                    .FirstOrDefaultAsync(x => x.ProductId == request.ProductId);

                if (stockItem is null)
                    return Results.NotFound("Stock item not found.");

                if (stockItem.QuantityAvailable < request.Quantity)
                    return Results.BadRequest("Not enough stock available.");

                stockItem.QuantityAvailable -= request.Quantity;
                stockItem.QuantityReserved += request.Quantity;
                stockItem.UpdatedAtUtc = DateTime.UtcNow;

                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Status = ReservationStatus.Pending,
                    CreatedAtUtc = DateTime.UtcNow,
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
                };

                db.Reservations.Add(reservation);

                await db.SaveChangesAsync();

                await hub.Clients.All.SendAsync("StockUpdated", new
                {
                    ProductId = stockItem.ProductId,
                    stockItem.QuantityAvailable,
                    stockItem.QuantityReserved
                });

                var response = new ReservationResponse(
                    reservation.Id,
                    reservation.ProductId,
                    reservation.Quantity,
                    reservation.Status.ToString(),
                    reservation.CreatedAtUtc,
                    reservation.ExpiresAtUtc
                );

                return Results.Created($"/api/reservations/{reservation.Id}", response);
            }
            finally
            {
                await lockService.ReleaseLockAsync(lockKey, lockValue);
            }
        })
         .RequireAuthorization(policy => policy.RequireRole("Admin", "Staff"));

        // To Confirm By Id
        group.MapPost("/{id:guid}/confirm", async (Guid id,
                SmartInventoryDbContext db , IHubContext<InventoryHub> hub) =>
        {
            var reservation = await db.Reservations
                .FirstOrDefaultAsync(x => x.Id == id);

            if (reservation is null)
                return Results.NotFound("Reservation not found.");

            if (reservation.Status != ReservationStatus.Pending)
                return Results.BadRequest("Only pending reservations can be confirmed.");

            var stockItem = await db.StockItems
                .FirstOrDefaultAsync(x => x.ProductId == reservation.ProductId);

            if (stockItem is null)
                return Results.NotFound("Stock item not found.");

            stockItem.QuantityReserved -= reservation.Quantity;
            stockItem.UpdatedAtUtc = DateTime.UtcNow;

            reservation.Status = ReservationStatus.Confirmed;

            await db.SaveChangesAsync();

            await hub.Clients.All.SendAsync("StockUpdated", new
            {
                ProductId = stockItem.ProductId,
                stockItem.QuantityAvailable,
                stockItem.QuantityReserved
            });

            return Results.Ok("Reservation confirmed.");
        })
         .RequireAuthorization(policy => policy.RequireRole("Admin", "Staff"));

        // To Cancel By Id
        group.MapPost("/{id:guid}/cancel", async (Guid id,
            SmartInventoryDbContext db , IHubContext<InventoryHub> hub) =>
        {
            var reservation = await db.Reservations
                .FirstOrDefaultAsync(x => x.Id == id);

            if (reservation is null)
                return Results.NotFound("Reservation not found.");

            if (reservation.Status != ReservationStatus.Pending)
                return Results.BadRequest("Only pending reservations can be cancelled.");

            var stockItem = await db.StockItems
                .FirstOrDefaultAsync(x => x.ProductId == reservation.ProductId);

            if (stockItem is null)
                return Results.NotFound("Stock item not found.");

            stockItem.QuantityReserved -= reservation.Quantity;
            stockItem.QuantityAvailable += reservation.Quantity;
            stockItem.UpdatedAtUtc = DateTime.UtcNow;

            reservation.Status = ReservationStatus.Cancelled;

            await db.SaveChangesAsync();

            await hub.Clients.All.SendAsync("StockUpdated", new
            {
                ProductId = stockItem.ProductId,
                stockItem.QuantityAvailable,
                stockItem.QuantityReserved
            });

            return Results.Ok("Reservation cancelled.");
        })
         .RequireAuthorization(policy => policy.RequireRole("Admin", "Staff"));

        // Get All Reservations
        group.MapGet("/", async (SmartInventoryDbContext db) =>
        {
            var reservations = await db.Reservations
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new ReservationResponse(
                    x.Id,
                    x.ProductId,
                    x.Quantity,
                    x.Status.ToString(),
                    x.CreatedAtUtc,
                    x.ExpiresAtUtc
                ))
                .ToListAsync();

            return Results.Ok(reservations);
        })
         .RequireAuthorization(policy => policy.RequireRole("Admin", "Staff"));

        // Get Reservation By Id
        group.MapGet("/{id:guid}", async (Guid id, SmartInventoryDbContext db) =>
        {
            var reservation = await db.Reservations
                .Where(x => x.Id == id)
                .Select(x => new ReservationResponse(
                    x.Id,
                    x.ProductId,
                    x.Quantity,
                    x.Status.ToString(),
                    x.CreatedAtUtc,
                    x.ExpiresAtUtc
                ))
                .FirstOrDefaultAsync();

            return reservation is null
                ? Results.NotFound()
                : Results.Ok(reservation);
        })
         .RequireAuthorization(policy => policy.RequireRole("Admin", "Staff"));

        return app;
    }
}
