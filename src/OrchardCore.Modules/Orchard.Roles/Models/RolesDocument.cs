using System.Collections.Generic;
using Orchard.Security;

namespace Orchard.Roles.Models
{
    public class RolesDocument
    {
        public int Id { get; set; }
        public List<Role> Roles { get; } = new List<Role>();
        public int Serial { get; set; }
    }
}
