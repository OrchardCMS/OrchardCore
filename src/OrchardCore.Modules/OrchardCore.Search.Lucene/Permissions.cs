using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageLuceneIndexes = LuceneIndexPermissionHelper.ManageLuceneIndexes;

    public static readonly Permission QueryLuceneApi = new("QueryLuceneApi", "Query Lucene Api", new[] { ManageLuceneIndexes });

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions =
            [
                ManageLuceneIndexes,
            ],
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions =
            [
                QueryLuceneApi,
            ],
        },
    ];

    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

    public Permissions(LuceneIndexSettingsService luceneIndexSettingsService)
    {
        _luceneIndexSettingsService = luceneIndexSettingsService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>
        {
            ManageLuceneIndexes,
            QueryLuceneApi
        };

        var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync();

        foreach (var index in luceneIndexSettings)
        {
            permissions.Add(LuceneIndexPermissionHelper.GetLuceneIndexPermission(index.IndexName));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
