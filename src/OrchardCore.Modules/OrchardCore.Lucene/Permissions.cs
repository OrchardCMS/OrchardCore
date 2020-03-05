using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Lucene
{
    public class Permissions : IPermissionProvider
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public static readonly Permission ManageIndexes = new Permission("ManageIndexes", "Manage Indexes");
        public static readonly Permission QueryLuceneApi = new Permission("QueryLuceneApi", "Query Lucene Api", new[] { ManageIndexes });

        public Permissions(LuceneIndexSettingsService luceneIndexSettingsService)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync();
            var result = new List<Permission>();
            foreach (var index in luceneIndexSettings)
            {
                var permission = new Permission("QueryLucene" + index.IndexName + "Index", "Query Lucene " + index.IndexName + " Index", new[] { ManageIndexes });
                result.Add(permission);
            }

            result.Add(ManageIndexes);
            result.Add(QueryLuceneApi);

            return result;
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
