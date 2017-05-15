using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Queries
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageQueries = new Permission("ManageQueries", "Manage queries");
        public static readonly Permission ExecuteQueryApi = new Permission("ExecuteQueryApi", "Execute Query Api", new[] { ManageQueries });

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageQueries };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageQueries }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageQueries, ExecuteQueryApi }
                }
            };
        }
    }
}