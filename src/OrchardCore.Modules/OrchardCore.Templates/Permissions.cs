using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Templates
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageTemplates = new Permission("ManageTemplates", "Manage templates", isSecurityCritical: true);

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
                    Name = BuiltInRole.Administrator,
                    Permissions = new[] { ManageTemplates }
                },
                new PermissionStereotype
                {
                    Name = BuiltInRole.Editor,
                    Permissions = new[] { ManageTemplates }
                }
            };
        }
    }
}
