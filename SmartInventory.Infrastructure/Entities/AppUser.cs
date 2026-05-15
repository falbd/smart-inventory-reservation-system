using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventory.Infrastructure.Entities
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty ;
        public string PasswordHash {  get; set; } = string.Empty ;
        public string Role {  get; set; } = "Staff" ;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow ;
    }
}
