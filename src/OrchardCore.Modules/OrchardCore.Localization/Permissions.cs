using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageCultures = new Permission("ManageCultures", "Manage supported culture");

        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageCultures;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageCultures }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { ManageCultures }
                }
            };
        }
    }
}
