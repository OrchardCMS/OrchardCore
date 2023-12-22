using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public class Permissions(AzureAIIndexSettingsService indexSettingsService) : IPermissionProvider
{
    private readonly AzureAIIndexSettingsService _indexSettingsService = indexSettingsService;

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes,
        };

        var elasticIndexSettings = await _indexSettingsService.GetSettingsAsync();

        foreach (var index in elasticIndexSettings)
        {
            permissions.Add(AzureAISearchIndexPermissionHelper.GetPermission(index.IndexName));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _defaultStereotypes;

    private static readonly IEnumerable<PermissionStereotype> _defaultStereotypes = new[]
        {
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes,
                },
            },
        };
}
