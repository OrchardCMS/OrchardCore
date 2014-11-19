using OrchardVNext.Environment.Extensions.Folders;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.Environment.Extensions.Models;
using OrchardVNext.Utility;
using OrchardVNext.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

using OrchardVNext.Localization;
using System.Reflection;

namespace OrchardVNext.Environment.Extensions {
    public class ExtensionManager : IExtensionManager {
        private readonly IEnumerable<IExtensionFolders> _folders;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        
        public Localizer T { get; set; }

        public ExtensionManager(
            IEnumerable<IExtensionFolders> folders,
            IEnumerable<IExtensionLoader> loaders) {

            _folders = folders;
            _loaders = loaders.OrderBy(x => x.Order).ToArray();
            T = NullLocalizer.Instance;
        }

        // This method does not load extension types, simply parses extension manifests from 
        // the filesystem. 
        public ExtensionDescriptor GetExtension(string id) {
            return AvailableExtensions().FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _folders.SelectMany(x => x.AvailableExtensions()).ToList();
        }

        public IEnumerable<FeatureDescriptor> AvailableFeatures() {
            return AvailableExtensions()
                    .SelectMany(ext => ext.Features)
                    .OrderByDependenciesAndPriorities(HasDependency, GetPriority)
                    .ToReadOnlyCollection();
        }

        internal static int GetPriority(FeatureDescriptor featureDescriptor) {
            return featureDescriptor.Priority;
        }

        /// <summary>
        /// Returns true if the item has an explicit or implicit dependency on the subject
        /// </summary>
        /// <param name="item"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        internal static bool HasDependency(FeatureDescriptor item, FeatureDescriptor subject) {
            if (DefaultExtensionTypes.IsTheme(item.Extension.ExtensionType)) {
                if (DefaultExtensionTypes.IsModule(subject.Extension.ExtensionType)) {
                    // Themes implicitly depend on modules to ensure build and override ordering
                    return true;
                }

                if (DefaultExtensionTypes.IsTheme(subject.Extension.ExtensionType)) {
                    // Theme depends on another if it is its base theme
                    return item.Extension.BaseTheme == subject.Id;
                }
            }

            // Return based on explicit dependencies
            return item.Dependencies != null &&
                   item.Dependencies.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x, subject.Id));
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
            Logger.Information("Loading features");

            var result = featureDescriptors
                .Select(descriptor => LoadFeature(descriptor))
                .ToArray();

            Logger.Information("Done loading features");
            return result;
        }

        private Feature LoadFeature(FeatureDescriptor featureDescriptor) {
            var extensionDescriptor = featureDescriptor.Extension;
            var featureId = featureDescriptor.Id;
            var extensionId = extensionDescriptor.Id;

            ExtensionEntry extensionEntry;
            try {
                extensionEntry = BuildEntry(extensionDescriptor);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error loading extension '{0}'", extensionId);
                throw new OrchardException(T("Error while loading extension '{0}'.", extensionId), ex);
            }

            if (extensionEntry == null) {
                // If the feature could not be compiled for some reason,
                // return a "null" feature, i.e. a feature with no exported types.
                return new Feature {
                    Descriptor = featureDescriptor,
                    ExportedTypes = Enumerable.Empty<Type>()
                };
            }

            var extensionTypes = extensionEntry.ExportedTypes.Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract);
            var featureTypes = new List<Type>();

            foreach (var type in extensionTypes) {
                string sourceFeature = GetSourceFeatureNameForType(type, extensionId);
                if (String.Equals(sourceFeature, featureId, StringComparison.OrdinalIgnoreCase)) {
                    featureTypes.Add(type);
                }
            }

            return new Feature {
                Descriptor = featureDescriptor,
                ExportedTypes = featureTypes
            };
        }

        private static string GetSourceFeatureNameForType(Type type, string extensionId) {
            foreach (OrchardFeatureAttribute featureAttribute in type.GetTypeInfo().GetCustomAttributes(typeof(OrchardFeatureAttribute), false)) {
                return featureAttribute.FeatureName;
            }
            return extensionId;
        }

        private ExtensionEntry BuildEntry(ExtensionDescriptor descriptor) {
            foreach (var loader in _loaders) {
                ExtensionEntry entry = loader.Load(descriptor);
                if (entry != null)
                    return entry;
            }

            Logger.Warning("No suitable loader found for extension \"{0}\"", descriptor.Id);
            return null;
        }
    }
}