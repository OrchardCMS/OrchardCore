using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ExecuteGraphQLMutations = new Permission("ExecuteGraphQLMutations", "Execute GraphQL Mutations.");
        public static readonly Permission ExecuteGraphQL = new Permission("ExecuteGraphQL", "Execute GraphQL.", new[] { ExecuteGraphQLMutations });

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult<IEnumerable<Permission>>(
                new[] {
                    ExecuteGraphQLMutations,
                    ExecuteGraphQL,
                }
            );
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
