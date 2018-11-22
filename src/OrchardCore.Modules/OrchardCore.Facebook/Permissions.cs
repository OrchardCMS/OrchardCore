using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageFacebookApp
            = new Permission(nameof(ManageFacebookApp), "View and edit the Facebook app.");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageFacebookApp;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageFacebookApp
                }
            };
        }
    }
}
