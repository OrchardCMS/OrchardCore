using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene;

public sealed class Permissions : IPermissionProvider
{
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

    [Obsolete("This will be removed in a future release. Instead use 'LuceneSearchPermissions.ManageLuceneIndexes'.")]
    public static readonly Permission ManageLuceneIndexes = LuceneIndexPermissionHelper.ManageLuceneIndexes;

    [Obsolete("This will be removed in a future release. Instead use 'LuceneSearchPermissions.QueryLuceneApi'.")]
    public static readonly Permission QueryLuceneApi = new("QueryLuceneApi", "Query Lucene Api", new[] { ManageLuceneIndexes });

    public Permissions(LuceneIndexSettingsService luceneIndexSettingsService)
    {
        _luceneIndexSettingsService = luceneIndexSettingsService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            LuceneSearchPermissions.ManageLuceneIndexes,
            LuceneSearchPermissions.QueryLuceneApi,
        };

        var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync();

        foreach (var index in luceneIndexSettings)
        {
            permissions.Add(LuceneIndexPermissionHelper.GetLuceneIndexPermission(index.IndexName));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions =
            [
                LuceneSearchPermissions.ManageLuceneIndexes,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                LuceneSearchPermissions.QueryLuceneApi,
            ],
        },
    ];
}
