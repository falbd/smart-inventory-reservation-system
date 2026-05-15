using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Contracts.Stock
{
    public sealed record AddStockRequest(
        int Quantity
    );
}
