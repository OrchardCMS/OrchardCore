using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ExecuteGraphQLMutations = new Permission("ExecuteGraphQLMutations", "Execute GraphQL Mutations.");
        public static readonly Permission ExecuteGraphQL = new Permission("ExecuteGraphQL", "Execute GraphQL.", new[] { ExecuteGraphQLMutations });

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                ExecuteGraphQLMutations,
                ExecuteGraphQL
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ExecuteGraphQLMutations }
                }
            };
        }
    }
}
