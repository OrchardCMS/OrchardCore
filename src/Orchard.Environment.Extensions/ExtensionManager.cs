using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.Localization;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.Utility;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Utility;

namespace Orchard.Environment.Extensions
{
    public class ExtensionManager : IExtensionManager
    {
        private readonly IExtensionLocator _extensionLocator;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly ILogger _logger;
        private List<ExtensionDescriptor> _availableExtensions;
        private List<FeatureDescriptor> _availableFeatures;

        private Dictionary<string, Feature> _features = new Dictionary<string, Feature>();

        public Localizer T { get; set; }

        public ExtensionManager(
            IExtensionLocator extensionLocator,
            IEnumerable<IExtensionLoader> loaders,
            ILogger<ExtensionManager> logger)
        {
            _extensionLocator = extensionLocator;
            _loaders = loaders.OrderBy(x => x.Order).ToArray();
            _logger = logger;
            T = NullLocalizer.Instance;
        }

        // This method does not load extension types, simply parses extension manifests from
        // the filesystem.
        public ExtensionDescriptor GetExtension(string id)
        {
            return AvailableExtensions().FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions()
        {
            // Memoize the list of extensions to prevent module discovery on every call
            if (_availableExtensions == null)
            {
                _availableExtensions = _extensionLocator.AvailableExtensions().ToList();
            }

            return _availableExtensions;
        }

        public IEnumerable<FeatureDescriptor> AvailableFeatures()
        {
            // Memoize the list of features to prevent re-ordering on every call
            if (_availableFeatures == null)
            {
                _availableFeatures = AvailableExtensions()
                    .SelectMany(ext => ext.Features)
                    .OrderByDependenciesAndPriorities(HasDependency, GetPriority)
                    .ToList();
            }

            return _availableFeatures;
        }

        internal static int GetPriority(FeatureDescriptor featureDescriptor)
        {
            return featureDescriptor.Priority;
        }

        /// <summary>
        /// Returns true if the item has an explicit or implicit dependency on the subject
        /// </summary>
        /// <param name="item"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public bool HasDependency(FeatureDescriptor item, FeatureDescriptor subject)
        {
            if (DefaultExtensionTypes.IsTheme(item.Extension.ExtensionType))
            {
                if (DefaultExtensionTypes.IsModule(subject.Extension.ExtensionType))
                {
                    // Themes implicitly depend on modules to ensure build and override ordering
                    return true;
                }

                if (DefaultExtensionTypes.IsTheme(subject.Extension.ExtensionType))
                {
                    // Theme depends on another if it is its base theme
                    return item.Extension.BaseTheme == subject.Id;
                }
            }

            // Return based on explicit dependencies
            return item.Dependencies != null &&
                   item.Dependencies.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x, subject.Id));
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Loading features");
            }

            var result = featureDescriptors
                .Select(descriptor => LoadFeature(descriptor))
                .ToArray();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done loading features");
            }
            return result;
        }

        private Feature LoadFeature(FeatureDescriptor featureDescriptor)
        {
            lock(_features)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loading feature {0}", featureDescriptor.Name);
                }

                if(_features.ContainsKey(featureDescriptor.Id))
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Feature {0} loaded from cache", featureDescriptor.Name);
                    }

                    return _features[featureDescriptor.Id];
                }

                var extensionDescriptor = featureDescriptor.Extension;
                var featureId = featureDescriptor.Id;
                var extensionId = extensionDescriptor.Id;

                ExtensionEntry extensionEntry;
                try
                {
                    extensionEntry = BuildEntry(extensionDescriptor);
                }
                catch (Exception ex)
                {
                    _logger.LogError(string.Format("Error loading extension '{0}'", extensionId), ex);
                    throw new OrchardException(T("Error while loading extension '{0}'.", extensionId), ex);
                }

                Feature feature;
                if (extensionEntry == null)
                {
                    // If the feature could not be compiled for some reason,
                    // return a "null" feature, i.e. a feature with no exported types.
                    feature = new Feature
                    {
                        Descriptor = featureDescriptor,
                        ExportedTypes = Enumerable.Empty<Type>()
                    };

                    _features.Add(featureDescriptor.Id, feature);
                    return feature;
                }

                var extensionTypes = extensionEntry.ExportedTypes.Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract);
                var featureTypes = new List<Type>();

                foreach (var type in extensionTypes)
                {
                    string sourceFeature = GetSourceFeatureNameForType(type, extensionId);
                    if (String.Equals(sourceFeature, featureId, StringComparison.OrdinalIgnoreCase))
                    {
                        featureTypes.Add(type);
                    }
                }

                feature = new Feature
                {
                    Descriptor = featureDescriptor,
                    ExportedTypes = featureTypes
                };

                _features.Add(featureDescriptor.Id, feature);
                return feature;
            }
        }

        private static string GetSourceFeatureNameForType(Type type, string extensionId)
        {
            foreach (OrchardFeatureAttribute featureAttribute in type.GetTypeInfo().GetCustomAttributes(typeof(OrchardFeatureAttribute), false))
            {
                return featureAttribute.FeatureName;
            }
            return extensionId;
        }

        private ExtensionEntry BuildEntry(ExtensionDescriptor descriptor)
        {
            foreach (var loader in _loaders)
            {
                ExtensionEntry entry = loader.Load(descriptor);
                if (entry != null)
                    return entry;
            }

            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("No suitable loader found for extension \"{0}\"", descriptor.Id);
            }
            return null;
        }
    }
}
