using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Twitter
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageTwitterLogin
            = new Permission(nameof(ManageTwitterLogin), "Manage Twitter Login settings");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageTwitterLogin;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageTwitterLogin
                }
            };
        }
    }
}
