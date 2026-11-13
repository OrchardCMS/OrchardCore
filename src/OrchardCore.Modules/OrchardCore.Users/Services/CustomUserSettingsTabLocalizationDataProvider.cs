using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Localization.Data;

namespace OrchardCore.Users.Services;

/// <summary>
/// Provides the names of the user editor tabs of the custom user settings, which are the display names of their types.
/// </summary>
internal sealed class CustomUserSettingsTabLocalizationDataProvider : ILocalizationDataProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public CustomUserSettingsTabLocalizationDataProvider(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(type => type.GetStereotype() == "CustomUserSettings" && !string.IsNullOrWhiteSpace(type.DisplayName))
            .Select(type => type.DisplayName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => new DataLocalizedString(TabAndCardNames.DataLocalizationContext, name, string.Empty))
            .ToArray();
}
