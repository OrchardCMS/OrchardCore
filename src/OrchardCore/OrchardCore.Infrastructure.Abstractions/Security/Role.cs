using System.Collections.Immutable;

namespace OrchardCore.Security
{
    public class Role: IRole
    {
        public string RoleName { get; set; }
        public string NormalizedRoleName { get; set; }
        public ImmutableArray<RoleClaim> RoleClaims { get; set; } = ImmutableArray<RoleClaim>.Empty;
    }
}
