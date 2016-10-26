using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;

namespace Orchard.Environment.Shell
{
    public class ShellFeaturesManager : IShellFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        private readonly ILogger<ShellFeaturesManager> _logger;

        public FeatureDependencyNotificationHandler FeatureDependencyNotification { get; set; }

        public ShellFeaturesManager(IExtensionManager extensionManager,
            IShellDescriptorManager shellDescriptorManager,
            ILogger<ShellFeaturesManager> logger,
            IStringLocalizer<ShellFeaturesManager> localizer)
        {
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;

            _logger = logger;
            T = localizer;
        }
        public IStringLocalizer T { get; set; }

        public IEnumerable<IFeatureInfo> EnabledFeatures()
        {
            return EnabledFeatures(_shellDescriptorManager.GetShellDescriptorAsync().Result);
        }

        public IEnumerable<IFeatureInfo> EnabledFeatures(ShellDescriptor shell)
        {
            var extensions = _extensionManager.GetExtensions();
            var features = extensions.Features;

            return features.Where(fd => shell.Features.Any(sf => sf.Name == fd.Id));
        }

        public IEnumerable<IFeatureInfo> EnableFeatures(IEnumerable<IFeatureInfo> features)
        {
            return EnableFeatures(features, false);
        }

        public IEnumerable<IFeatureInfo> EnableFeatures(IEnumerable<IFeatureInfo> features, bool force)
        {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptorAsync().Result;
            var enabledFeatures = EnabledFeatures(shellDescriptor).ToList();

            var extensions = _extensionManager.GetExtensions();
            
            IDictionary<IFeatureInfo, bool> availableFeatures = extensions
                .Features
                .ToDictionary(featureDescriptor => featureDescriptor,
                                featureDescriptor => enabledFeatures.FirstOrDefault(shellFeature => shellFeature.Name == featureDescriptor.Id) != null);

            IEnumerable<IFeatureInfo> featuresToEnable = features
                .Select(feature => EnableFeature(feature, availableFeatures, false)).ToList()
                .SelectMany(ies => ies.Select(s => s));

            if (featuresToEnable.Any())
            {
                foreach (var feature in featuresToEnable)
                {
                    enabledFeatures.Add(feature);
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("{0} was enabled", feature.Id);
                    }
                }

                _shellDescriptorManager.UpdateShellDescriptorAsync(
                    shellDescriptor.SerialNumber,
                    shellDescriptor.Features,
                    shellDescriptor.Parameters);
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        public IEnumerable<IFeatureInfo> DisableFeatures(IEnumerable<IFeatureInfo> features)
        {
            return DisableFeatures(features, false);
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="features">The features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        public IEnumerable<IFeatureInfo> DisableFeatures(IEnumerable<IFeatureInfo> features, bool force)
        {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptorAsync().Result;
            List<ShellFeature> enabledFeatures = shellDescriptor.Features.ToList();

            var featuresToDisable = new List<IFeatureInfo>();
            foreach (var feature in features)
            {
                var disabled = DisableFeature(feature, force);
                featuresToDisable.AddRange(disabled);
            }

            if (featuresToDisable.Any())
            {
                foreach (IFeatureInfo feature in featuresToDisable)
                {
                    enabledFeatures.RemoveAll(shellFeature => shellFeature.Name == feature.Id);
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("{0} was disabled", feature.Id);
                    }
                }

                _shellDescriptorManager.UpdateShellDescriptorAsync(
                    shellDescriptor.SerialNumber,
                    shellDescriptor.Features,
                    shellDescriptor.Parameters);
            }

            return featuresToDisable;
        }

        /// <summary>
        /// Lists all enabled features that depend on a given feature.
        /// </summary>
        /// <param name="feature">feature to check.</param>
        /// <returns>An enumeration with dependent feature IDs.</returns>
        public IEnumerable<string> GetDependentFeatures(IFeatureInfo feature)
        {
            var getEnabledDependants =
                new Func<string, IDictionary<IFeatureInfo, bool>, IDictionary<IFeatureInfo, bool>>(
                    (currentFeatureId, fs) => fs
                        .Where(f => f.Value && f.Key.Dependencies != null && f.Key.Dependencies
                            .Select(s => s.ToLowerInvariant())
                            .Contains(currentFeatureId.ToLowerInvariant()))
                        .ToDictionary(f => f.Key, f => f.Value));

            var shellDescriptor = _shellDescriptorManager.GetShellDescriptorAsync().Result;
            var enabledFeatures = EnabledFeatures(shellDescriptor).ToList();

            var extensions = _extensionManager.GetExtensions();

            IDictionary<IFeatureInfo, bool> availableFeatures = extensions
                .Features
                .ToDictionary(featureDescriptor => featureDescriptor,
                                featureDescriptor => enabledFeatures.FirstOrDefault(shellFeature => shellFeature.Name == featureDescriptor.Id) != null);

            return GetAffectedFeatures(feature.Id, availableFeatures, getEnabledDependants);
        }

        /// <summary>
        /// Enables a feature.
        /// </summary>
        /// <param name="featureId">The ID of the feature to be enabled.</param>
        /// <param name="availableFeatures">A dictionary of the available feature descriptors and their current state (enabled / disabled).</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration of the enabled features.</returns>
        private IEnumerable<IFeatureInfo> EnableFeature(IFeatureInfo featureInfo, 
            IDictionary<IFeatureInfo, bool> availableFeatures, bool force)
        {
            var getDisabledDependencies =
                new Func<string, IDictionary<IFeatureInfo, bool>, IDictionary<IFeatureInfo, bool>>(
                    (currentFeatureId, featuresState) =>
                    {
                        KeyValuePair<IFeatureInfo, bool> feature = featuresState.Single(featureState => featureState.Key.Id.Equals(currentFeatureId, StringComparison.OrdinalIgnoreCase));

                        // Retrieve disabled dependencies for the current feature
                        return feature.Key.Dependencies
                                      .Select(fId =>
                                      {
                                          var states = featuresState.Where(featureState => featureState.Key.Id.Equals(fId, StringComparison.OrdinalIgnoreCase)).ToList();

                                          if (states.Count == 0)
                                          {
                                              throw new OrchardException(T["Failed to get state for feature {0}", fId]);
                                          }

                                          if (states.Count > 1)
                                          {
                                              throw new OrchardException(T["Found {0} states for feature {1}", states.Count, fId]);
                                          }

                                          return states[0];
                                      })
                                      .Where(featureState => !featureState.Value)
                                      .ToDictionary(f => f.Key, f => f.Value);
                    });

            IEnumerable<string> affectedFeatures = 
                GetAffectedFeatures(featureInfo.Id, availableFeatures, getDisabledDependencies);

            var extensions = _extensionManager.GetExtensions();
            var featuresToEnable = extensions.Features.Where(x => affectedFeatures.Contains(x.Id));

            if (featuresToEnable.Count() > 1 && !force)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("Additional features need to be enabled.");
                }
                if (FeatureDependencyNotification != null)
                {
                    FeatureDependencyNotification("If {0} is enabled, then you'll also need to enable {1}.", featureInfo, featuresToEnable.Where(f => f.Id != featureInfo.Id));
                }

                return Enumerable.Empty<IFeatureInfo>();
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a feature.
        /// </summary>
        /// <param name="featureId">The ID of the feature to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration of the disabled features.</returns>
        private IEnumerable<IFeatureInfo> DisableFeature(IFeatureInfo featureInfo, bool force)
        {
            IEnumerable<string> affectedFeatures = 
                GetDependentFeatures(featureInfo);

            var extensions = _extensionManager.GetExtensions();
            var featuresToDisable = extensions.Features.Where(x => affectedFeatures.Contains(x.Id));

            if (featuresToDisable.Count() > 1 && !force)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("Additional features need to be disabled.");
                }
                if (FeatureDependencyNotification != null)
                {
                    FeatureDependencyNotification("If {0} is disabled, then you'll also need to disable {1}.", featureInfo, featuresToDisable.Where(f => f.Id != featureInfo.Id));
                }

                return Enumerable.Empty<IFeatureInfo>();
            }

            return featuresToDisable;
        }

        private static IEnumerable<string> GetAffectedFeatures(
            string featureId, IDictionary<IFeatureInfo, bool> features,
            Func<string, IDictionary<IFeatureInfo, bool>, IDictionary<IFeatureInfo, bool>> getAffectedDependencies)
        {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { featureId };
            var stack = new Stack<IDictionary<IFeatureInfo, bool>>();

            stack.Push(getAffectedDependencies(featureId, features));

            while (stack.Any())
            {
                var next = stack.Pop();
                foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency.Key.Id)))
                {
                    dependencies.Add(dependency.Key.Id);
                    stack.Push(getAffectedDependencies(dependency.Key.Id, features));
                }
            }

            return dependencies;
        }
    }
}
