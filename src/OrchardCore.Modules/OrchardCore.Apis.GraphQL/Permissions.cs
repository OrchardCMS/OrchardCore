using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ExecuteGraphQL = new Permission("ExecuteGraphQL", "Execute GraphQL.");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(GetPermissions());
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

        private IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                ExecuteGraphQL
            };
        }
    }
}
