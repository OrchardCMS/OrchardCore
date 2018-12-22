using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageGoogleAuthentication
            = new Permission(nameof(ManageGoogleAuthentication), "Manage Google Authentication settings");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageGoogleAuthentication;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageGoogleAuthentication
                }
            };
        }
    }
}
