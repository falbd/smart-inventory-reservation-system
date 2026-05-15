using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Contracts.Reservations
{
    public sealed record ReservationResponse(
        Guid Id,
        Guid ProductId,
        int Quantity,
        string Status,
        DateTime CreatedAtUtc,
        DateTime ExpiresAtUtc
     );
}
