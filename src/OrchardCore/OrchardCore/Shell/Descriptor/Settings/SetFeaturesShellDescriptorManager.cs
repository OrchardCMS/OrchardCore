using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Descriptor.Settings;

/// <summary>
/// Implements <see cref="IShellDescriptorManager"/> by returning a single tenant with a specified set
/// of features. This class can be registered as a singleton as its state never changes.
/// </summary>
public class SetFeaturesShellDescriptorManager : IShellDescriptorManager
{
    private readonly IEnumerable<ShellFeature> _shellFeatures;
    private readonly IExtensionManager _extensionManager;

    private ShellDescriptor _shellDescriptor;

    public SetFeaturesShellDescriptorManager(IEnumerable<ShellFeature> shellFeatures, IExtensionManager extensionManager)
    {
        _shellFeatures = shellFeatures;
        _extensionManager = extensionManager;
    }

    public async Task<ShellDescriptor> GetShellDescriptorAsync()
    {
        if (_shellDescriptor == null)
        {
            var features = GetImplicitlyEnabledShellFeatures()
                .Concat(_shellFeatures)
                .Distinct()
                .ToArray();
            var featureIds = features.Select(sf => sf.Id);

            var missingDependencies = (await _extensionManager.LoadFeaturesAsync(featureIds))
                .Select(entry => entry.Id)
                .Except(featureIds)
                .Select(id => new ShellFeature(id));

            _shellDescriptor = new ShellDescriptor
            {
                Features = features
                    .Concat(missingDependencies)
                    .ToList(),
            };
        }

        return _shellDescriptor;
    }

    public Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures)
    {
        return Task.CompletedTask;
    }

    private IEnumerable<ShellFeature> GetImplicitlyEnabledShellFeatures()
    {
        return _extensionManager
            .GetFeatures()
            .Where(feature => feature.IsImplicitlyEnabled)
            .Select(feature => new ShellFeature(feature.Id, alwaysEnabled: true));
    }
}
