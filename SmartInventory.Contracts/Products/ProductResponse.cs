using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Contracts.Products
{
    public sealed record ProductResponse(
        Guid Id,
        string Name,
        string Sku,
        decimal Price,
        int QuantityAvailable,
        int QuantityReserved
    );
}
