using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Themes
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ApplyTheme = new Permission("ApplyTheme") { Description = "Apply a Theme" };

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                ApplyTheme,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ApplyTheme }
                },
            };
        }
    }
}
