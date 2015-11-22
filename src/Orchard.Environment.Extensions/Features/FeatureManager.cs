using Microsoft.Extensions.Logging;
using Orchard.Localization;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public class FeatureManager : IFeatureManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly ILogger _logger;

        /// <summary>
        /// Delegate to notify about feature dependencies.
        /// </summary>
        public FeatureDependencyNotificationHandler FeatureDependencyNotification { get; set; }

        public FeatureManager(
            IExtensionManager extensionManager,
            IShellDescriptorManager shellDescriptorManager,
            ILoggerFactory loggerFactory)
        {
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;
            _logger = loggerFactory.CreateLogger<FeatureManager>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <summary>
        /// Retrieves the available features.
        /// </summary>
        /// <returns>An enumeration of feature descriptors for the available features.</returns>
        public IEnumerable<FeatureDescriptor> GetAvailableFeatures()
        {
            return _extensionManager.AvailableFeatures();
        }

        /// <summary>
        /// Retrieves the enabled features.
        /// </summary>
        /// <returns>An enumeration of feature descriptors for the enabled features.</returns>
        public IEnumerable<FeatureDescriptor> GetEnabledFeatures()
        {
            var currentShellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            return _extensionManager.EnabledFeatures(currentShellDescriptor);
        }

        /// <summary>
        /// Retrieves the disabled features.
        /// </summary>
        /// <returns>An enumeration of feature descriptors for the disabled features.</returns>
        public IEnumerable<FeatureDescriptor> GetDisabledFeatures()
        {
            var currentShellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            return _extensionManager.DisabledFeatures(currentShellDescriptor);
        }

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        public IEnumerable<string> EnableFeatures(IEnumerable<string> featureIds)
        {
            return EnableFeatures(featureIds, false);
        }

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        public IEnumerable<string> EnableFeatures(IEnumerable<string> featureIds, bool force)
        {
            ShellDescriptor shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            List<ShellFeature> enabledFeatures = shellDescriptor.Features.ToList();

            IDictionary<FeatureDescriptor, bool> availableFeatures = GetAvailableFeatures()
                .ToDictionary(featureDescriptor => featureDescriptor,
                                featureDescriptor => enabledFeatures.FirstOrDefault(shellFeature => shellFeature.Name == featureDescriptor.Id) != null);

            IEnumerable<string> featuresToEnable = featureIds
                .Select(featureId => EnableFeature(featureId, availableFeatures, force)).ToList()
                .SelectMany(ies => ies.Select(s => s));

            if (featuresToEnable.Count() > 0)
            {
                foreach (string featureId in featuresToEnable)
                {
                    string id = featureId;

                    enabledFeatures.Add(new ShellFeature { Name = id });
                    _logger.LogInformation("{0} was enabled", featureId);
                }

                _shellDescriptorManager.UpdateShellDescriptor(shellDescriptor.SerialNumber, enabledFeatures,
                                                              shellDescriptor.Parameters);
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        public IEnumerable<string> DisableFeatures(IEnumerable<string> featureIds)
        {
            return DisableFeatures(featureIds, false);
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        public IEnumerable<string> DisableFeatures(IEnumerable<string> featureIds, bool force)
        {
            ShellDescriptor shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            List<ShellFeature> enabledFeatures = shellDescriptor.Features.ToList();

            IEnumerable<string> featuresToDisable = featureIds
                .Select(featureId => DisableFeature(featureId, force)).ToList()
                .SelectMany(ies => ies.Select(s => s));

            if (featuresToDisable.Any())
            {
                foreach (string featureId in featuresToDisable)
                {
                    string id = featureId;

                    enabledFeatures.RemoveAll(shellFeature => shellFeature.Name == id);
                    _logger.LogInformation("{0} was disabled", featureId);
                }

                _shellDescriptorManager.UpdateShellDescriptor(shellDescriptor.SerialNumber, enabledFeatures,
                                                              shellDescriptor.Parameters);
            }

            return featuresToDisable;
        }

        /// <summary>
        /// Lists all enabled features that depend on a given feature.
        /// </summary>
        /// <param name="featureId">ID of the feature to check.</param>
        /// <returns>An enumeration with dependent feature IDs.</returns>
        public IEnumerable<string> GetDependentFeatures(string featureId)
        {
            var getEnabledDependants =
                new Func<string, IDictionary<FeatureDescriptor, bool>, IDictionary<FeatureDescriptor, bool>>(
                    (currentFeatureId, fs) => fs
                        .Where(f => f.Value && f.Key.Dependencies != null && f.Key.Dependencies
                            .Select(s => s.ToLowerInvariant())
                            .Contains(currentFeatureId.ToLowerInvariant()))
                        .ToDictionary(f => f.Key, f => f.Value));

            ShellDescriptor shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            List<ShellFeature> enabledFeatures = shellDescriptor.Features.ToList();

            IDictionary<FeatureDescriptor, bool> availableFeatures = GetAvailableFeatures()
                .ToDictionary(featureDescriptor => featureDescriptor,
                              featureDescriptor => enabledFeatures.FirstOrDefault(shellFeature => shellFeature.Name.Equals(featureDescriptor.Id)) != null);

            return GetAffectedFeatures(featureId, availableFeatures, getEnabledDependants);
        }

        /// <summary>
        /// Enables a feature.
        /// </summary>
        /// <param name="featureId">The ID of the feature to be enabled.</param>
        /// <param name="availableFeatures">A dictionary of the available feature descriptors and their current state (enabled / disabled).</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration of the enabled features.</returns>
        private IEnumerable<string> EnableFeature(string featureId, IDictionary<FeatureDescriptor, bool> availableFeatures, bool force)
        {
            var getDisabledDependencies =
                new Func<string, IDictionary<FeatureDescriptor, bool>, IDictionary<FeatureDescriptor, bool>>(
                    (currentFeatureId, featuresState) =>
                    {
                        KeyValuePair<FeatureDescriptor, bool> feature = featuresState.Single(featureState => featureState.Key.Id.Equals(currentFeatureId, StringComparison.OrdinalIgnoreCase));

                        // Retrieve disabled dependencies for the current feature
                        return feature.Key.Dependencies
                                      .Select(fId =>
                                      {
                                          var states = featuresState.Where(featureState => featureState.Key.Id.Equals(fId, StringComparison.OrdinalIgnoreCase)).ToList();

                                          if (states.Count == 0)
                                          {
                                              throw new OrchardException(T("Failed to get state for feature {0}", fId));
                                          }

                                          if (states.Count > 1)
                                          {
                                              throw new OrchardException(T("Found {0} states for feature {1}", states.Count, fId));
                                          }

                                          return states[0];
                                      })
                                      .Where(featureState => !featureState.Value)
                                      .ToDictionary(f => f.Key, f => f.Value);
                    });

            IEnumerable<string> featuresToEnable = GetAffectedFeatures(featureId, availableFeatures, getDisabledDependencies);
            if (featuresToEnable.Count() > 1 && !force)
            {
                _logger.LogWarning("Additional features need to be enabled.");
                if (FeatureDependencyNotification != null)
                {
                    FeatureDependencyNotification("If {0} is enabled, then you'll also need to enable {1}.", featureId, featuresToEnable.Where(fId => fId != featureId));
                }

                return Enumerable.Empty<string>();
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a feature.
        /// </summary>
        /// <param name="featureId">The ID of the feature to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration of the disabled features.</returns>
        private IEnumerable<string> DisableFeature(string featureId, bool force)
        {
            IEnumerable<string> featuresToDisable = GetDependentFeatures(featureId);

            if (featuresToDisable.Count() > 1 && !force)
            {
                _logger.LogWarning("Additional features need to be disabled.");
                if (FeatureDependencyNotification != null)
                {
                    FeatureDependencyNotification("If {0} is disabled, then you'll also need to disable {1}.", featureId, featuresToDisable.Where(fId => fId != featureId));
                }

                return Enumerable.Empty<string>();
            }

            return featuresToDisable;
        }

        private static IEnumerable<string> GetAffectedFeatures(
            string featureId, IDictionary<FeatureDescriptor, bool> features,
            Func<string, IDictionary<FeatureDescriptor, bool>, IDictionary<FeatureDescriptor, bool>> getAffectedDependencies)
        {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { featureId };
            var stack = new Stack<IDictionary<FeatureDescriptor, bool>>();

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