using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

/// <summary>
/// Provides the tab and card names of placement for translation, as listed by the
/// <see cref="ITabAndCardNameContributor"/> implementations, each name once.
/// </summary>
public sealed class TabAndCardLocalizationDataProvider : ILocalizationDataProvider
{
    private readonly IEnumerable<ITabAndCardNameContributor> _contributors;

    public TabAndCardLocalizationDataProvider(IEnumerable<ITabAndCardNameContributor> contributors)
    {
        _contributors = contributors;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        // The group of the shapes placed in no tab or card is named implicitly.
        var names = new HashSet<string>(StringComparer.Ordinal) { TabAndCardNames.Default };

        foreach (var contributor in _contributors)
        {
            foreach (var name in await contributor.GetNamesAsync())
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }
        }

        return names
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select(name => new DataLocalizedString(TabAndCardNames.DataLocalizationContext, name, string.Empty));
    }
}
