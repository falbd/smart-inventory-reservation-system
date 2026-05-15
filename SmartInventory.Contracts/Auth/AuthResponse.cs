using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Contracts.Auth
{
    public sealed record AuthResponse(
        Guid UserId,
        string FullName,
        string Email,
        string Role,
        string Token
   );
}
