using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Environment.Shell
{
    //public class ShellStateCoordinator : IShellDescriptorManagerEventHandler
    //{
    //    private readonly ShellSettings _settings;
    //    private readonly IShellStateManager _stateManager;
    //    private readonly IExtensionManager _extensionManager;
    //    private readonly IProcessingEngine _processingEngine;
    //    private readonly IFeatureEventHandler _featureEvents;

    //    public ShellStateCoordinator(
    //        ShellSettings settings,
    //        IShellStateManager stateManager,
    //        IExtensionManager extensionManager,
    //        IProcessingEngine processingEngine,
    //        IFeatureEventHandler featureEvents)
    //    {
    //        _settings = settings;
    //        _stateManager = stateManager;
    //        _extensionManager = extensionManager;
    //        _processingEngine = processingEngine;
    //        _featureEvents = featureEvents;
    //        Logger = NullLogger.Instance;
    //    }

    //    public ILogger Logger { get; set; }

    //    void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant)
    //    {
    //        // deduce and apply state changes involved
    //        var shellState = _stateManager.GetShellState();
    //        foreach (var feature in descriptor.Features)
    //        {
    //            var featureName = feature.Name;
    //            var featureState = shellState.Features.SingleOrDefault(f => f.Name == featureName);
    //            if (featureState == null)
    //            {
    //                featureState = new ShellFeatureState
    //                {
    //                    Name = featureName
    //                };
    //                shellState.Features = shellState.Features.Concat(new[] { featureState });
    //            }
    //            if (!featureState.IsInstalled)
    //            {
    //                _stateManager.UpdateInstalledState(featureState, ShellFeatureState.State.Rising);
    //            }
    //            if (!featureState.IsEnabled)
    //            {
    //                _stateManager.UpdateEnabledState(featureState, ShellFeatureState.State.Rising);
    //            }
    //        }
    //        foreach (var featureState in shellState.Features)
    //        {
    //            var featureName = featureState.Name;
    //            if (descriptor.Features.Any(f => f.Name == featureName))
    //            {
    //                continue;
    //            }
    //            if (!featureState.IsDisabled)
    //            {
    //                _stateManager.UpdateEnabledState(featureState, ShellFeatureState.State.Falling);
    //            }
    //        }

    //        FireApplyChangesIfNeeded();
    //    }

    //    private void FireApplyChangesIfNeeded()
    //    {
    //        var shellState = _stateManager.GetShellState();
    //        if (shellState.Features.Any(FeatureIsChanging))
    //        {
    //            var descriptor = new ShellDescriptor
    //            {
    //                Features = shellState.Features
    //                    .Where(FeatureShouldBeLoadedForStateChangeNotifications)
    //                    .Select(x => new ShellFeature
    //                    {
    //                        Name = x.Name
    //                    })
    //                    .ToArray()
    //            };

    //            Logger.Information("Adding pending task 'ApplyChanges' for shell '{0}'", _settings.Name);
    //            _processingEngine.AddTask(
    //                _settings,
    //                descriptor,
    //                "IShellStateManagerEventHandler.ApplyChanges",
    //                new Dictionary<string, object>());
    //        }
    //    }

    //    private static bool FeatureIsChanging(ShellFeatureState shellFeatureState)
    //    {
    //        if (shellFeatureState.EnableState == ShellFeatureState.State.Rising ||
    //            shellFeatureState.EnableState == ShellFeatureState.State.Falling)
    //        {
    //            return true;
    //        }
    //        if (shellFeatureState.InstallState == ShellFeatureState.State.Rising ||
    //            shellFeatureState.InstallState == ShellFeatureState.State.Falling)
    //        {
    //            return true;
    //        }
    //        return false;
    //    }

    //    private static bool FeatureShouldBeLoadedForStateChangeNotifications(ShellFeatureState shellFeatureState)
    //    {
    //        return FeatureIsChanging(shellFeatureState) || shellFeatureState.EnableState == ShellFeatureState.State.Up;
    //    }

    //    void IShellStateManagerEventHandler.ApplyChanges()
    //    {
    //        Logger.Information("Applying changes for for shell '{0}'", _settings.Name);

    //        var shellState = _stateManager.GetShellState();

    //        // start with description of all declared features in order - order preserved with all merging
    //        var orderedFeatureDescriptors = _extensionManager.AvailableFeatures();

    //        // merge feature state into ordered list
    //        var orderedFeatureDescriptorsAndStates = orderedFeatureDescriptors
    //            .Select(featureDescriptor => new {
    //                FeatureDescriptor = featureDescriptor,
    //                FeatureState = shellState.Features.FirstOrDefault(s => s.Name == featureDescriptor.Id),
    //            })
    //            .Where(entry => entry.FeatureState != null)
    //            .ToArray();

    //        // get loaded feature information
    //        var loadedFeatures = _extensionManager.LoadFeatures(orderedFeatureDescriptorsAndStates.Select(entry => entry.FeatureDescriptor)).ToArray();

    //        // merge loaded feature information into ordered list
    //        var loadedEntries = orderedFeatureDescriptorsAndStates.Select(
    //            entry => new {
    //                Feature = loadedFeatures.SingleOrDefault(f => f.Descriptor == entry.FeatureDescriptor)
    //                          ?? new Feature
    //                          {
    //                              Descriptor = entry.FeatureDescriptor,
    //                              ExportedTypes = Enumerable.Empty<Type>()
    //                          },
    //                entry.FeatureDescriptor,
    //                entry.FeatureState,
    //            }).ToList();

    //        // find feature state that is beyond what's currently available from modules
    //        var additionalState = shellState.Features.Except(loadedEntries.Select(entry => entry.FeatureState));

    //        // create additional stub entries for the sake of firing state change events on missing features
    //        var allEntries = loadedEntries.Concat(additionalState.Select(featureState => {
    //            var featureDescriptor = new FeatureDescriptor
    //            {
    //                Id = featureState.Name,
    //                Extension = new ExtensionDescriptor
    //                {
    //                    Id = featureState.Name
    //                }
    //            };
    //            return new
    //            {
    //                Feature = new Feature
    //                {
    //                    Descriptor = featureDescriptor,
    //                    ExportedTypes = Enumerable.Empty<Type>(),
    //                },
    //                FeatureDescriptor = featureDescriptor,
    //                FeatureState = featureState
    //            };
    //        })).ToArray();

    //        // lower enabled states in reverse order
    //        foreach (var entry in allEntries.Reverse().Where(entry => entry.FeatureState.EnableState == ShellFeatureState.State.Falling))
    //        {
    //            Logger.Information("Disabling feature '{0}'", entry.Feature.Descriptor.Id);
    //            _featureEvents.Disabling(entry.Feature);
    //            _stateManager.UpdateEnabledState(entry.FeatureState, ShellFeatureState.State.Down);
    //            _featureEvents.Disabled(entry.Feature);
    //        }

    //        // lower installed states in reverse order
    //        foreach (var entry in allEntries.Reverse().Where(entry => entry.FeatureState.InstallState == ShellFeatureState.State.Falling))
    //        {
    //            Logger.Information("Uninstalling feature '{0}'", entry.Feature.Descriptor.Id);
    //            _featureEvents.Uninstalling(entry.Feature);
    //            _stateManager.UpdateInstalledState(entry.FeatureState, ShellFeatureState.State.Down);
    //            _featureEvents.Uninstalled(entry.Feature);
    //        }

    //        // raise install and enabled states in order
    //        foreach (var entry in allEntries.Where(entry => IsRising(entry.FeatureState)))
    //        {
    //            if (entry.FeatureState.InstallState == ShellFeatureState.State.Rising)
    //            {
    //                Logger.Information("Installing feature '{0}'", entry.Feature.Descriptor.Id);
    //                _featureEvents.Installing(entry.Feature);
    //                _stateManager.UpdateInstalledState(entry.FeatureState, ShellFeatureState.State.Up);
    //                _featureEvents.Installed(entry.Feature);
    //            }
    //            if (entry.FeatureState.EnableState == ShellFeatureState.State.Rising)
    //            {
    //                Logger.Information("Enabling feature '{0}'", entry.Feature.Descriptor.Id);
    //                _featureEvents.Enabling(entry.Feature);
    //                _stateManager.UpdateEnabledState(entry.FeatureState, ShellFeatureState.State.Up);
    //                _featureEvents.Enabled(entry.Feature);
    //            }
    //        }

    //        // re-fire if any event handlers initiated additional state changes
    //        FireApplyChangesIfNeeded();
    //    }

    //    static bool IsRising(ShellFeatureState state)
    //    {
    //        return state.InstallState == ShellFeatureState.State.Rising ||
    //               state.EnableState == ShellFeatureState.State.Rising;
    //    }
    //}
}
