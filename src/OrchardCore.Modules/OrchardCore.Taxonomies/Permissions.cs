using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Taxonomies
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageTaxonomies = new("ManageTaxonomy", "Manage taxonomies");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageTaxonomies }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageTaxonomies },
                },
            };
        }
    }
}
