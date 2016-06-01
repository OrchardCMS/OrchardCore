using System.Collections.Generic;

namespace Orchard.Users.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string NormalizedRoleName { get; set; }
        public List<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
    }
}
