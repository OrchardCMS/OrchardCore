using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.Security
{
    public class Role : IRole
    {
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string NormalizedRoleName { get; set; }
        public List<RoleClaim> RoleClaims { get; } = new List<RoleClaim>();
    }
}
