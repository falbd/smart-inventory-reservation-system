using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Contracts.Auth
{
    public sealed record RegisterRequest(
        string FullName,
        string Email,
        string Password,
        string Role
   );
}
