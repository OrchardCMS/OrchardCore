using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Builders;

public class CompositionStrategy : ICompositionStrategy
{
    private readonly IExtensionManager _extensionManager;
    private readonly ITypeFeatureProvider _typeFeatureProvider;
    private readonly ILogger _logger;

    public CompositionStrategy(
        IExtensionManager extensionManager,
        ITypeFeatureProvider typeFeatureProvider,
        ILogger<CompositionStrategy> logger)
    {
        _extensionManager = extensionManager;
        _typeFeatureProvider = typeFeatureProvider;
        _logger = logger;
    }

    public async Task<ShellBlueprint> ComposeAsync(ShellSettings settings, ShellDescriptor descriptor)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Composing blueprint");
        }

        var featureNames = descriptor.Features.Select(x => x.Id).ToHashSet();

        var features = await _extensionManager.LoadFeaturesAsync(featureNames);

        var entries = new Dictionary<Type, IEnumerable<IFeatureInfo>>();

        foreach (var feature in features)
        {
            if (feature.DefaultTenantOnly && !settings.IsDefaultShell())
            {
                _logger.LogError("Skipping feature '{FeatureName}' as it is allowed on the default tenant only.", feature.Id);

                continue;
            }

            foreach (var exportedType in _typeFeatureProvider.GetTypesForFeature(feature))
            {
                var requiredFeatures = RequireFeaturesAttribute.GetRequiredFeatureNamesForType(exportedType);

                if (!requiredFeatures.All(x => featureNames.Contains(x)))
                {
                    continue;
                }

                entries[exportedType] = entries.TryGetValue(exportedType, out var featureDependencies)
                    ? featureDependencies.Append(feature).ToArray()
                    : [feature];

            }
        }

        var result = new ShellBlueprint
        {
            Settings = settings,
            Descriptor = descriptor,
            Dependencies = entries,
        };

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Done composing blueprint");

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Shell blueprint for tenant '{TenantName}' contains {TypeCount} type(s)", settings.Name, entries.Count);

                foreach (var entry in entries)
                {
                    _logger.LogTrace("Type '{TypeName}' is provided by feature(s): {FeatureNames}", entry.Key.FullName, string.Join(", ", entry.Value.Select(f => f.Id)));
                }

                foreach (var feature in entries.Values.SelectMany(f => f).Select(f => f.Id).Distinct().OrderBy(f => f))
                {
                    _logger.LogTrace("Enabled feature: '{FeatureId}'", feature);
                }
            }
        }

        return result;
    }
}
