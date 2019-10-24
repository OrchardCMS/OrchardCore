using System.Collections.Immutable;

namespace OrchardCore.Security
{
    public class Role : IRole
    {
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string NormalizedRoleName { get; set; }
        public ImmutableArray<RoleClaim> RoleClaims { get; set; } = ImmutableArray<RoleClaim>.Empty;

        /// <summary>
        /// Creates a shallow copy of this role.
        /// </summary>
        public virtual Role Clone()
        {
            return new Role()
            {
                RoleName = RoleName,
                NormalizedRoleName = NormalizedRoleName,
                RoleClaims = RoleClaims
            };
        }
    }
}
