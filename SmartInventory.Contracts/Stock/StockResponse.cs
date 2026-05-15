using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Contracts.Stock
{
    public sealed record StockResponse(
        Guid ProductId,
        string ProductName,
        string Sku,
        int QuantityAvailable,
        int QuantityReserved,
        int TotalQuantity,
        DateTime UpdatedAtUtc
    );
}
