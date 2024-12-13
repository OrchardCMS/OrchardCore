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

        var featureNames = descriptor.Features.Select(x => x.Id).ToArray();

        var features = await _extensionManager.LoadFeaturesAsync(featureNames);

        var entries = new Dictionary<Type, IEnumerable<IFeatureInfo>>();

        foreach (var feature in features)
        {
            foreach (var exportedType in _typeFeatureProvider.GetTypesForFeature(feature))
            {
                var requiredFeatures = RequireFeaturesAttribute.GetRequiredFeatureNamesForType(exportedType);

                if (requiredFeatures.All(x => featureNames.Contains(x)))
                {
                    if (entries.TryGetValue(exportedType, out var featureDependencies))
                    {
                        featureDependencies = featureDependencies.Append(feature).ToArray();
                    }
                    else
                    {
                        featureDependencies = [feature];
                    }

                    entries[exportedType] = featureDependencies;
                }
            }
        }

        var result = new ShellBlueprint
        {
            Settings = settings,
            Descriptor = descriptor,
            Dependencies = entries
        };

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Done composing blueprint");
        }

        return result;
    }
}
