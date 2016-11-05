using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.DisplayManagement.Notify;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Modules.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Modules.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IFeatureManager _featureManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly INotifier _notifier;

        public ModuleService(
                IFeatureManager featureManager,
                IExtensionManager extensionManager,
                IShellDescriptorManager shellDescriptorManager,
                IShellFeaturesManager shellFeaturesManager,
                IHtmlLocalizer<AdminMenu> htmlLocalizer,
                INotifier notifier)
        {
            _notifier = notifier;
            _featureManager = featureManager;
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;
            _shellFeaturesManager = shellFeaturesManager;
            //if (_featureManager.FeatureDependencyNotification == null) {
            //    _featureManager.FeatureDependencyNotification = GenerateWarning;
            //}

            T = htmlLocalizer;
        }

        public IHtmlLocalizer T { get; set; }

        /// <summary>
        /// Retrieves an enumeration of the available features together with its state (enabled / disabled).
        /// </summary>
        /// <returns>An enumeration of the available features together with its state (enabled / disabled).</returns>
        public async Task<IEnumerable<ModuleFeature>> GetAvailableFeatures()
        {
            var currentShellDescriptor = await _shellDescriptorManager.GetShellDescriptorAsync();
            var enabledFeatures = currentShellDescriptor.Features;
            return _extensionManager
                .LoadFeatures(_extensionManager.GetExtensions().Features)
                .Select(f => AssembleModuleFromDescriptor(f, enabledFeatures
                    .FirstOrDefault(sf => string.Equals(sf.Name, f.FeatureInfo.Name, StringComparison.OrdinalIgnoreCase)) != null));
        }

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        public void EnableFeatures(IEnumerable<string> featureIds)
        {
            EnableFeatures(featureIds, false);
        }

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        public void EnableFeatures(IEnumerable<string> featureIds, bool force)
        {
            var featuresToEnable = _extensionManager
                .GetExtensions()
                .Features
                .Where(x => featureIds.Contains(x.Id));

            var features = _shellFeaturesManager.EnableFeatures(featuresToEnable, force);

            foreach (var feature in features)
            {
                var featureName = _shellFeaturesManager
                    .EnabledFeatures()
                    .First(f => f.Id.Equals(feature.Id, StringComparison.OrdinalIgnoreCase))
                    .Name;

                _notifier.Success(T["{0} was enabled", featureName]);
            }
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        public void DisableFeatures(IEnumerable<string> featureIds)
        {
            DisableFeatures(featureIds, false);
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        public void DisableFeatures(IEnumerable<string> featureIds, bool force)
        {
            var availableFeatures = _extensionManager.GetExtensions().Features;
            var featuresToDisable = availableFeatures.Where(x => featureIds.Contains(x.Id));
            var features = _shellFeaturesManager.DisableFeatures(featuresToDisable, force);
            foreach (var feature in features)
            {
                var featureName = availableFeatures[feature.Id].Name;

                _notifier.Success(T["{0} was disabled", featureName]);
            }
        }

        ///// <summary>
        ///// Determines if a module was recently installed by using the project's last written time.
        ///// </summary>
        ///// <param name="extensionDescriptor">The extension descriptor.</param>
        //public bool IsRecentlyInstalled(ExtensionDescriptor extensionDescriptor) {
        //    DateTime lastWrittenUtc = _cacheManager.Get(extensionDescriptor, descriptor => {
        //        string projectFile = GetManifestPath(extensionDescriptor);
        //        if (!string.IsNullOrEmpty(projectFile)) {
        //            // If project file was modified less than 24 hours ago, the module was recently deployed
        //            return _virtualPathProvider.GetFileLastWriteTimeUtc(projectFile);
        //        }

        //        return DateTime.UtcNow;
        //    });

        //    return DateTime.UtcNow.Subtract(lastWrittenUtc) < new TimeSpan(1, 0, 0, 0);
        //}

        public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId)
        {
            var dependants = _shellFeaturesManager.GetDependentFeatures(featureId);
            return _extensionManager
                .GetExtensions()
                .Features
                .Where(x => dependants.Contains(x.Id))
                .ToList();
        }

        ///// <summary>
        ///// Retrieves the full path of the manifest file for a module's extension descriptor.
        ///// </summary>
        ///// <param name="extensionDescriptor">The module's extension descriptor.</param>
        ///// <returns>The full path to the module's manifest file.</returns>
        //private string GetManifestPath(ExtensionDescriptor extensionDescriptor) {
        //    string projectPath = _virtualPathProvider.Combine(extensionDescriptor.Location, extensionDescriptor.Id, "module.txt");

        //    if (!_virtualPathProvider.FileExists(projectPath)) {
        //        return null;
        //    }

        //    return projectPath;
        //}

        private static ModuleFeature AssembleModuleFromDescriptor(FeatureEntry feature, bool isEnabled)
        {
            return new ModuleFeature
            {
                Descriptor = feature.FeatureInfo,
                IsEnabled = isEnabled
            };
        }

        //private void GenerateWarning(string messageFormat, string featureName, IEnumerable<string> featuresInQuestion) {
        //    if (featuresInQuestion.Count() < 1)
        //        return;

        //    Services.Notifier.Warning(T(
        //        messageFormat,
        //        featureName,
        //        featuresInQuestion.Count() > 1
        //            ? string.Join("",
        //                          featuresInQuestion.Select(
        //                              (fn, i) =>
        //                              T(i == featuresInQuestion.Count() - 1
        //                                    ? "{0}"
        //                                    : (i == featuresInQuestion.Count() - 2
        //                                           ? "{0} and "
        //                                           : "{0}, "), fn).ToString()).ToArray())
        //            : featuresInQuestion.First()));
        //}
    }
}