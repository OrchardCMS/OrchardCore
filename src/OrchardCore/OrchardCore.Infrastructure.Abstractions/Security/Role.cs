using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.Security
{
    public class Role : IRole
    {
        /// <summary>
        /// True if the object can't be used to update the database.
        /// </summary>
        [JsonIgnore]
        public bool IsReadonly { get; set; }

        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string NormalizedRoleName { get; set; }
        public List<RoleClaim> RoleClaims { get; } = new List<RoleClaim>();
    }
}
