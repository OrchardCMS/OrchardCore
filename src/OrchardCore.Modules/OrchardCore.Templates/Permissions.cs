using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Templates
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageTemplates = new Permission("ManageTemplates", "Manage templates");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageTemplates };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageTemplates }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { ManageTemplates }
                }
            };
        }
    }
}