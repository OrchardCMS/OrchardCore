using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Orchard.Lucene
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageIndexes = new Permission("ManageIndexes") { Description = "Manage Indexes" };

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] 
            {
                ManageIndexes
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageIndexes }
                }
            };
        }
    }
}