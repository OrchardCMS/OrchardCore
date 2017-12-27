using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageWorkflows = new Permission("ManageWorkflows", "Manage workflows");

        public IEnumerable<Permission> GetPermissions()
        {
            return new List<Permission> { ManageWorkflows };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageWorkflows }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageWorkflows }
                }
            };
        }
    }
}