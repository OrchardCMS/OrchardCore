using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Templates
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageTemplates = new("ManageTemplates", "Manage templates", isSecurityCritical: true);

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageTemplates }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = RoleNames.Administrator,
                    Permissions = new[] { ManageTemplates },
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Editor,
                    Permissions = new[] { ManageTemplates },
                },
            };
        }
    }
}
