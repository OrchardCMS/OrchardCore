using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public sealed class Permissions : IPermissionProvider
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService;

    public Permissions(AzureAISearchIndexSettingsService indexSettingsService)
    {
        _indexSettingsService = indexSettingsService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes,
        };

        var indexSettings = await _indexSettingsService.GetSettingsAsync();

        foreach (var index in indexSettings)
        {
            permissions.Add(AzureAISearchIndexPermissionHelper.GetPermission(index.IndexName));
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
                AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes,
            ],
        },
    ];
}
