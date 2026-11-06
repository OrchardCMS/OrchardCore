using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Users.Services;

/// <summary>
/// Lists the names of the user editor tabs of the custom user settings, which are the display names of their types.
/// </summary>
internal sealed class CustomUserSettingsTabAndCardNameContributor : ITabAndCardNameContributor
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public CustomUserSettingsTabAndCardNameContributor(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<string>> GetNamesAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(type => type.GetStereotype() == "CustomUserSettings")
            .Select(type => type.DisplayName)
            .ToArray();
}
