using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Microsoft.Authentication
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAuthentication
            = new Permission(nameof(ManageAuthentication), "View and edit Authentication Providers.");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageAuthentication;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageAuthentication
                }
            };
        }
    }
}
