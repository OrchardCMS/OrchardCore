using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenIdConnect
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ConfigureOpenIdConnect = new Permission("ConfigureOpenIdConnect", "Configure OpenId Connect Settings");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ConfigureOpenIdConnect;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[] { ConfigureOpenIdConnect }
            };
        }

    }
}
