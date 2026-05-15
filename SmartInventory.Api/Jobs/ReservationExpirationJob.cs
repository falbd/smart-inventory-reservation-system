using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Api.Hubs;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Entities;

namespace SmartInventory.Api.Jobs;

public class ReservationExpirationJob
{
    private readonly SmartInventoryDbContext _db;
    private readonly IHubContext<InventoryHub> _hub;

    public ReservationExpirationJob(
        SmartInventoryDbContext db,
        IHubContext<InventoryHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task ReleaseExpiredReservationsAsync()
    {
        var now = DateTime.UtcNow;

        var expiredReservations = await _db.Reservations
            .Where(x =>
                x.Status == ReservationStatus.Pending &&
                x.ExpiresAtUtc <= now)
            .ToListAsync();

        foreach (var reservation in expiredReservations)
        {
            var stockItem = await _db.StockItems
                .FirstOrDefaultAsync(x => x.ProductId == reservation.ProductId);

            if (stockItem is null)
                continue;

            stockItem.QuantityReserved -= reservation.Quantity;
            stockItem.QuantityAvailable += reservation.Quantity;
            stockItem.UpdatedAtUtc = DateTime.UtcNow;

            reservation.Status = ReservationStatus.Expired;

            await _hub.Clients.All.SendAsync("StockUpdated", new
            {
                stockItem.ProductId,
                stockItem.QuantityAvailable,
                stockItem.QuantityReserved
            });
        }

        await _db.SaveChangesAsync();
    }
}