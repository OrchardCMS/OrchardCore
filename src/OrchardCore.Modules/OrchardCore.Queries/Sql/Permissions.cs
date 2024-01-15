using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries.Sql
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageSqlQueries = new("ManageSqlQueries", "Manage SQL Queries");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageSqlQueries
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = RoleNames.Administrator,
                    Permissions = new[] { ManageSqlQueries }
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Editor,
                    Permissions = new[] { ManageSqlQueries }
                }
            };
        }
    }
}
