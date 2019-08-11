using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Email
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageEmailSettings = new Permission("ManageEmailSettings", "Manage Email Settings");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] 
            {
                ManageEmailSettings,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageEmailSettings }
                },
            };
        }
    }
}