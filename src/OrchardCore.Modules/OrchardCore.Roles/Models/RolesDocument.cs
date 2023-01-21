using System.Collections.Generic;
using OrchardCore.Data.Documents;
using OrchardCore.Security;

namespace OrchardCore.Roles.Models
{
    public class RolesDocument : Document
    {
        public List<Role> Roles { get; set; } = new();
        public Dictionary<string, List<string>> MissingFeaturesByRole { get; set; } = new();
    }
}
