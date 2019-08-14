using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ReverseProxy
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ReverseProxySettings = new Permission("ReverseProxySettings", "Manage Reverse Proxy Settings");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ReverseProxySettings;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ReverseProxySettings }
                },
            };
        }
    }
}