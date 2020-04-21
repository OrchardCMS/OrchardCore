using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageQueries = new Permission("ManageQueries", "Manage queries");
        public static readonly Permission ExecuteApiAll = new Permission("ExecuteApiAll", "Execute Api - All queries", new[] { ManageQueries });

        private static readonly Permission ExecuteApi = new Permission("ExecuteApi_{0}", "Execute Api - {0}", new[] { ManageQueries, ExecuteApiAll });

        private readonly IQueryManager _queryManager;

        public Permissions(IQueryManager queryManager)
        {
            _queryManager = queryManager;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission> { ManageQueries, ExecuteApiAll };

            foreach (var query in await _queryManager.ListQueriesAsync())
            {
                list.Add(CreatePermissionForQuery(query.Name));
            }

            return list;
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
                    Permissions = new[] { ManageQueries }
                }
            };
        }

        public static Permission CreatePermissionForQuery(string name)
        {
            return new Permission(
                    String.Format(ExecuteApi.Name, name),
                    String.Format(ExecuteApi.Description, name),
                    ExecuteApi.ImpliedBy
                );
        }
    }
}
