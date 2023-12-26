using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public class Permissions(AzureAISearchIndexSettingsService indexSettingsService) : IPermissionProvider
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService = indexSettingsService;

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
