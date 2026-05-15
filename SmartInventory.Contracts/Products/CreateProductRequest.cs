using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Contracts.Products
{
    public sealed record CreateProductRequest(
        string Name,
        string Sku,
        decimal Price,
        int InitialStock
     );
}
