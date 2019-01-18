using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Taxonomies
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageTaxonomies = new Permission("ManageTaxonomy", "Manage taxonomies");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageTaxonomies };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageTaxonomies }
                }
            };
        }
    }
}