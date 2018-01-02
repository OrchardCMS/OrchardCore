using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ExecuteGraphQL = new Permission("ExecuteGraphQL", "Execute GraphQL.");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                ExecuteGraphQL
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = GetPermissions()
                }
            };
        }
    }
}
