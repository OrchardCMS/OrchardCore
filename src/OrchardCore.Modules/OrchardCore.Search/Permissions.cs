using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission QuerySearchIndex = new("QuerySearchIndex", "Query any index");

        public static readonly Permission ManageSearchSettings = new("ManageSearchSettings", "Manage Search Settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageSearchSettings }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageSearchSettings },
                },
            };
        }
    }
}
