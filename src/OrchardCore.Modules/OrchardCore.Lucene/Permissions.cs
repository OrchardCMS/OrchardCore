using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Lucene
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageIndexes = new Permission("ManageIndexes", "Manage Indexes");
        public static readonly Permission QueryLuceneApi = new Permission("QueryLuceneApi", "Query Lucene Api", new[] { ManageIndexes });
        public static readonly Permission QueryLuceneSearch = new Permission("QueryLuceneSearch", "Query Lucene Search", new[] { ManageIndexes });

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] 
            {
                ManageIndexes,
                QueryLuceneApi,
                QueryLuceneSearch
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