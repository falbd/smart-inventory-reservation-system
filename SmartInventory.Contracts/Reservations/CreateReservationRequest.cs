using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Contracts.Reservations
{
    public sealed record CreateReservationRequest(
        Guid ProductId,
        int Quantity
    );
}
