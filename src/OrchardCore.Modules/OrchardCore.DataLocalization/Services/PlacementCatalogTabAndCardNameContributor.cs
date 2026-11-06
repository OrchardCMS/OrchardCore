using System.Reflection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;

namespace OrchardCore.DataLocalization.Services;

/// <summary>
/// Lists the tab and card names of the placement catalogs generated at build time, for the types of the enabled
/// features and the types they derive from.
/// </summary>
public sealed class PlacementCatalogTabAndCardNameContributor : ITabAndCardNameContributor
{
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly ITypeFeatureProvider _typeFeatureProvider;

    public PlacementCatalogTabAndCardNameContributor(
        IShellFeaturesManager shellFeaturesManager,
        ITypeFeatureProvider typeFeatureProvider)
    {
        _shellFeaturesManager = shellFeaturesManager;
        _typeFeatureProvider = typeFeatureProvider;
    }

    public async Task<IEnumerable<string>> GetNamesAsync()
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

        var names = new List<string>();

        foreach (var assembly in types.Select(type => type.Assembly).Distinct())
        {
            foreach (var catalog in assembly.GetCustomAttributes<PlacementCatalogAttribute>())
            {
                if (catalog.DeclaringType is null || !types.Contains(catalog.DeclaringType))
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

        return names;
    }
}
