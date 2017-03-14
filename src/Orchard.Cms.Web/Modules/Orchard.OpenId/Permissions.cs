using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.OpenId
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageOpenIdApplications = new Permission("ManageOpenIdApplications", "Managing Open ID Connect Applications");
        
        public IEnumerable<Permission> GetPermissions()
        {
            yield return ManageOpenIdApplications;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[] { ManageOpenIdApplications }
            };
        }
    }
}
