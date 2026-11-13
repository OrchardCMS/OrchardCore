using System.Reflection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

/// <summary>
/// Provides the tab and card names of the placement catalogs generated at build time, for the types of the enabled
/// features and the types they derive from, and the default group that shapes placed in no tab or card share.
/// </summary>
internal sealed class PlacementCatalogLocalizationDataProvider : ILocalizationDataProvider
{
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly ITypeFeatureProvider _typeFeatureProvider;

    public PlacementCatalogLocalizationDataProvider(
        IShellFeaturesManager shellFeaturesManager,
        ITypeFeatureProvider typeFeatureProvider)
    {
        _shellFeaturesManager = shellFeaturesManager;
        _typeFeatureProvider = typeFeatureProvider;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        // A location that a base type sets counts for the enabled types derived from it, whatever their feature.
        var types = new HashSet<Type>();

        foreach (var feature in await _shellFeaturesManager.GetEnabledFeaturesAsync())
        {
            foreach (var type in _typeFeatureProvider.GetTypesForFeature(feature))
            {
                for (var current = type; current is not null; current = current.BaseType)
                {
                    types.Add(current.IsGenericType ? current.GetGenericTypeDefinition() : current);
                }
            }
        }

        // The group of the shapes placed in no tab or card is named implicitly.
        var names = new List<string> { TabAndCardNames.Default };

        foreach (var assembly in types.Select(type => type.Assembly).Distinct())
        {
            foreach (var catalog in assembly.GetCustomAttributes<PlacementCatalogAttribute>())
            {
                // The catalog names a type of its own assembly, private nested types included.
                var declaringType = string.IsNullOrEmpty(catalog.DeclaringTypeName)
                    ? null
                    : assembly.GetType(catalog.DeclaringTypeName, throwOnError: false);

                if (declaringType is null || !types.Contains(declaringType))
                {
                    continue;
                }

                foreach (var location in catalog.Locations)
                {
                    names.AddRange(TabAndCardNames.FromLocation(location));
                }

                names.AddRange(catalog.Tabs);
                names.AddRange(catalog.Cards);
            }
        }

        return names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => new DataLocalizedString(TabAndCardNames.DataLocalizationContext, name, string.Empty));
    }
}
