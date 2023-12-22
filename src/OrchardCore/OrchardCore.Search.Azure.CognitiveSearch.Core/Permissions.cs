using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Search.Azure.CognitiveSearch.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Azure.CognitiveSearch;

public class Permissions : IPermissionProvider
{
    private readonly AzureCognitiveSearchIndexSettingsService _cognitiveSearchIndexSettingsService;

    public Permissions(AzureCognitiveSearchIndexSettingsService cognitiveSearchIndexSettingsService)
    {
        _cognitiveSearchIndexSettingsService = cognitiveSearchIndexSettingsService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes,
        };

        var elasticIndexSettings = await _cognitiveSearchIndexSettingsService.GetSettingsAsync();

        foreach (var index in elasticIndexSettings)
        {
            permissions.Add(AzureCognitiveSearchIndexPermissionHelper.GetElasticIndexPermission(index.IndexName));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return new[]
        {
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes,
                },
            },
        };
    }
}
