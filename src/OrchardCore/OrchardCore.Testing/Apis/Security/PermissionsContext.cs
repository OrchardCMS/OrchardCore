using System.Collections.Generic;
using System.Linq;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Testing.Apis.Security
{
    public class PermissionsContext
    {
        public PermissionsContext()
        {
            AuthorizedPermissions = Enumerable.Empty<Permission>();
            UsePermissionsContext = false;
        }
        public IEnumerable<Permission> AuthorizedPermissions { get; set; }

        public bool UsePermissionsContext { get; set; }
    }
}
