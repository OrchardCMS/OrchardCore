using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Lucene
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageIndexes = new Permission("ManageIndexes", "Manage Indexes");
        public static readonly Permission QueryLuceneApi = new Permission("QueryLuceneApi", "Query Lucene Api", new[] { ManageIndexes });
        public static readonly Permission QueryLuceneSearch = new Permission("QueryLuceneSearch", "Query Lucene Search", new[] { ManageIndexes });

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageIndexes,
                QueryLuceneApi,
                QueryLuceneSearch
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageIndexes }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { QueryLuceneApi }
                }
            };
        }
    }
}
