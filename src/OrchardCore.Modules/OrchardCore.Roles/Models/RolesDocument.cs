using System.Collections.Immutable;
using OrchardCore.Security;

namespace OrchardCore.Roles.Models
{
    public class RolesDocument
    {
        public int Id { get; set; }
        public ImmutableArray<Role> Roles { get; set; } = ImmutableArray<Role>.Empty;
        public int Serial { get; set; }
    }
}
