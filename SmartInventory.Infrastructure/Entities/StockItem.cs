using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Infrastructure.Entities

{
    public class StockItem
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }

        public int QuantityAvailable { get; set; }

        public int QuantityReserved { get; set; }

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = null!;
    }
}
