using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Infrastructure.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ExecuteGraphQLMutations = CommonPermissions.ExecuteGraphQLMutations;
        public static readonly Permission ExecuteGraphQL = CommonPermissions.ExecuteGraphQL;

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult<IEnumerable<Permission>>(
                new[]
                {
                    ExecuteGraphQLMutations,
                    ExecuteGraphQL,
                }
            );
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype
                {
                    Name = RoleNames.Administrator,
                    Permissions = new[] { ExecuteGraphQLMutations },
                }
            };
        }
    }
}
