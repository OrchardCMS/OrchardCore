using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene
{
    public class Permissions : IPermissionProvider
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public static readonly Permission ManageLuceneIndexes = new Permission("ManageLuceneIndexes", "Manage Lucene Indexes");
        public static readonly Permission QueryLuceneApi = new Permission("QueryLuceneApi", "Query Lucene Api", new[] { ManageLuceneIndexes });

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
                var permission = new Permission("QueryLucene" + index.IndexName + "Index", "Query Lucene " + index.IndexName + " Index", new[] { ManageLuceneIndexes });
                result.Add(permission);
            }

            result.Add(ManageLuceneIndexes);
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
                    Permissions = new[] { ManageLuceneIndexes }
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
