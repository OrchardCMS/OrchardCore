using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageWorkflows = new("ManageWorkflows", "Manage workflows", isSecurityCritical: true);
        public static readonly Permission ExecuteWorkflows = new("ExecuteWorkflows", "Execute workflows", isSecurityCritical: true);

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageWorkflows, ExecuteWorkflows }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageWorkflows, ExecuteWorkflows },
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { ManageWorkflows, ExecuteWorkflows },
                }
            };
        }
    }
}
