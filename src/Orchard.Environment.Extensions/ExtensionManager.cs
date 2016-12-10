using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Utility;

// Because there are different options I asked Sebastien how to organize using directives
// Sebastien told me that at Microsoft they put all System directives at the beginning.

namespace Orchard.Environment.Extensions
{
    public class ExtensionManager : IExtensionManager
    {
        private readonly ExtensionOptions _extensionOptions;
        private readonly IExtensionProvider _extensionProvider;
        private readonly IExtensionLoader _extensionLoader;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        private readonly ConcurrentDictionary<string, Task<ExtensionEntry>> _extensions
            = new ConcurrentDictionary<string, Task<ExtensionEntry>>();

        private readonly ConcurrentDictionary<string, Task<FeatureEntry>> _features
            = new ConcurrentDictionary<string, Task<FeatureEntry>>();

        private ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _featureDependencies
            = new ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>>();

        private ConcurrentDictionary<string, IEnumerable<IFeatureInfo>> _dependentFeatures
            = new ConcurrentDictionary<string, IEnumerable<IFeatureInfo>>();

        private IDictionary<string, IExtensionInfo> _extensionsById;

        private ConcurrentBag<IFeatureInfo> _allOrderedFeatureInfos;
        private ConcurrentBag<IFeatureInfo> _allUnorderedFeatureInfos;

        public ExtensionManager(
            IOptions<ExtensionOptions> optionsAccessor,
            IEnumerable<IExtensionProvider> extensionProviders,
            IEnumerable<IExtensionLoader> extensionLoaders,
            IHostingEnvironment hostingEnvironment,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<ExtensionManager> logger,
            IStringLocalizer<ExtensionManager> localizer)
        {
            _extensionOptions = optionsAccessor.Value;
            _extensionProvider = new CompositeExtensionProvider(extensionProviders);
            _extensionLoader = new CompositeExtensionLoader(extensionLoaders);
            _hostingEnvironment = hostingEnvironment;
            _typeFeatureProvider = typeFeatureProvider;
            L = logger;
            T = localizer;
        }
        public ILogger L { get; set; }
        public IStringLocalizer T { get; set; }

        public IExtensionInfo GetExtension(string extensionId)
        {
            GetExtensions(); // initialize

            return _extensionsById[extensionId];
        }

        public IEnumerable<IExtensionInfo> GetExtensions()
        {
            if (_extensionsById == null)
            {
                var extensionsById = new Dictionary<string, IExtensionInfo>();

                foreach (var searchPath in _extensionOptions.SearchPaths)
                {
                    foreach (var subDirectory in _hostingEnvironment
                        .ContentRootFileProvider
                        .GetDirectoryContents(searchPath)
                        .Where(x => x.IsDirectory))
                    {
                        var extensionId = subDirectory.Name;

                        if (!extensionsById.ContainsKey(extensionId))
                        {
                            var subPath = Path.Combine(searchPath, extensionId);

                            var extensionInfo =
                                _extensionProvider.GetExtensionInfo(subPath);

                            if (extensionInfo.ExtensionFileInfo.Exists)
                            {
                                extensionsById.Add(extensionId, extensionInfo);
                            }
                        }
                    }
                }

                _extensionsById = extensionsById;
            }

            return _extensionsById.Values;
        }

        public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId)
        {
            // Note: it was the wrong dictionary.

            // I've learned from Sebastien that here, even if only one value is used,
            // the factory can be executed multiple times. One solution is to use lazy.
            // See below as an example for this dictionary (not done for other ones).

            // But here things now are cached and fast, so maybe not worth to do this
            // I think I will revert this change, I just keep it there as an example

            return _featureDependencies.GetOrAdd(featureId,
                new Lazy<IEnumerable<IFeatureInfo>>(() =>
            {
                var unorderedFeatures = GetAllUnorderedFeatures();
                //var features = _extensionsById.Values.SelectMany(x => x.Features);

                var feature = unorderedFeatures.FirstOrDefault(x => x.Id == featureId);
                if (feature == null)
                {
                    return Enumerable.Empty<IFeatureInfo>();
                }

                var dependencies = new HashSet<IFeatureInfo>() { feature };
                var stack = new Stack<IFeatureInfo[]>();

                stack.Push(unorderedFeatures.Where(subject => feature.Dependencies.Any(x => x == subject.Id)).ToArray());

                while (stack.Count > 0)
                {
                    var next = stack.Pop();
                    foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                    {
                        dependencies.Add(dependency);
                        stack.Push(unorderedFeatures.Where(subject => feature.Dependencies.Any(x => x == subject.Id)).ToArray());
                    }
                }

                return dependencies.Distinct();
            })).Value;
        }

        public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId)
        {
            return _dependentFeatures.GetOrAdd(featureId, (key) =>
            {
                var unorderedFeatures = GetAllUnorderedFeatures();

                var feature = unorderedFeatures.FirstOrDefault(x => x.Id == key);
                if (feature == null)
                {
                    return Enumerable.Empty<IFeatureInfo>();
                }

                var getDependants =
                    new Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]>(
                        (currentFeature, fs) => fs
                            .Where(f =>
                                    f.Dependencies.Any(dep => dep == currentFeature.Id)
                                   ).ToArray());

                return
                    GetDependentFeatures(feature, unorderedFeatures.ToArray(), getDependants);

            });
        }

        private IList<IFeatureInfo> GetDependentFeatures(
            IFeatureInfo feature,
            IFeatureInfo[] features,
            Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]> getAffectedDependencies)
        {
            var dependencies = new HashSet<IFeatureInfo>() { feature };
            var stack = new Stack<IFeatureInfo[]>();

            stack.Push(getAffectedDependencies(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                {
                    dependencies.Add(dependency);
                    stack.Push(getAffectedDependencies(dependency, features));
                }
            }

            return dependencies.ToList();
        }

        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            // Results are cached so that there is no mismatch when loading an assembly twice.
            // Otherwise the same types would not match.
            return _extensions.GetOrAdd(extensionInfo.Id, id =>
            {
                var extension = _extensionLoader.Load(extensionInfo);

                if (extension.IsError && L.IsEnabled(LogLevel.Warning))
                {

                    L.LogError("No suitable loader found for extension \"{0}\"", id);
                }

                return Task.FromResult(extension);
            });
        }

        public async Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync()
        {
            // get loaded feature information
            var loadedFeatures = await Task.WhenAll(GetFeatures()
                .Select(feature => LoadFeatureAsync(feature))
                .ToArray());

            return loadedFeatures.AsEnumerable();
        }

        public IEnumerable<IFeatureInfo> GetFeatures()
        {
            if (_allOrderedFeatureInfos == null)
            {
                var allUnorderedFeatures = GetAllUnorderedFeatures();

                // When resolving dependencies we need to go through the dependencies graph.
                // But when we just order things, with the OrderBy utility, it's not the same.
                // Because all features and their dependencies are already in the collection.
                // And OrderByDependenciesAndPriorities() will do the job for all of them.
                // This according to the hasDependency and getPriority func we pass.
                // So I think here we don't need GetDependentFeatures().

                var orderedFeatureDescriptors = allUnorderedFeatures
                    .OrderByDependenciesAndPriorities(
                        // The hasDependency func could be this if only explicit dependencies
                        //(fiObv, fiSub) => fiObv.Dependencies?.Contains(fiSub.Id) ?? false,
                        HasDependency, // see below
                        (fi) => fi.Priority)
                    .Distinct();

                _allOrderedFeatureInfos = new ConcurrentBag<IFeatureInfo>(orderedFeatureDescriptors);
            }

            return _allOrderedFeatureInfos;
        }

        public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad)
        {
            // I think there is no need to order things if we use GetFeatures() which is ordered
            var allDependencies = featureIdsToLoad.SelectMany(featureId => GetFeatureDependencies(featureId));
            var orderedFeatureDescriptors = GetFeatures().Where(f => allDependencies.Any(d => d.Id == f.Id));

            return orderedFeatureDescriptors;
        }

        // Temporary code just given as an example to show how implicit dependencies are done in O1.
        // This ordering is important in different places e.g for the calling order of some handlers.
        // Another goal is not to rewrite this code when we use OrderByDependenciesAndPriorities() elsewhere.
        // So we would need to add it to the interface, but it's not the right place to be aware of theming.

        // I've seen the ThemeFeatureBuilderEvents where the BaseTheme is implemented.
        // But we also need the implicit dependencies of a theme on all other modules.
        // We just need them to order things, not when loading / enabling a feature.

        // Maybe add to the feature interface an HasDependency property which is a Func.
        // Then in a builder event we could init it to do the right job if it is a theme.

        // The property could be an array of func, then we could add a specific one when building.
        // So the global HasDependency could call all of them, and return true as soon one return true.

        internal static bool HasDependency(IFeatureInfo item, IFeatureInfo subject)
        {
            if (item.Extension.Manifest?.Type?.Equals("theme", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                if (subject.Id == "Core")
                {
                    return true;
                }

                if (subject.Extension?.Manifest?.Type?.Equals("module", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    return true;
                }

                if (subject.Extension.Manifest?.Type?.Equals("theme", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    var baseTheme = item.Extension.Manifest?.ConfigurationRoot["basetheme"];

                    if (baseTheme != null && baseTheme.Length != 0)
                    {
                        return baseTheme == subject.Id;
                    }
                }
            }

            return item.Dependencies?.Contains(subject.Id) ?? false;
        }

        public async Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(string[] featureIdsToLoad)
        {
            var features = GetFeatures(featureIdsToLoad);

            // get loaded feature information
            var loadedFeatures = await Task.WhenAll(features
                .Select(feature => LoadFeatureAsync(feature))
                .ToArray());

            return loadedFeatures.AsEnumerable();
        }

        private Task<FeatureEntry> LoadFeatureAsync(IFeatureInfo feature)
        {
            return _features.GetOrAdd(feature.Id, async (id) =>
            {
                var loadedExtension = await LoadExtensionAsync(feature.Extension);

                if (loadedExtension.IsError)
                {
                    return new NonCompiledFeatureEntry(feature);
                }

                var extensionTypes = loadedExtension
                    .ExportedTypes
                    .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract);

                var featureTypes = new List<Type>();

                foreach (var type in extensionTypes)
                {
                    string sourceFeature = GetSourceFeatureNameForType(type, feature.Id);
                    if (sourceFeature == id)
                    {
                        featureTypes.Add(type);
                        _typeFeatureProvider.TryAdd(type, feature);
                    }
                }

                return new CompiledFeatureEntry(feature, featureTypes);
            });
        }

        private IEnumerable<IFeatureInfo> GetAllUnorderedFeatures()
        {
            if (_allUnorderedFeatureInfos == null)
            {
                _allUnorderedFeatureInfos = new ConcurrentBag<IFeatureInfo>(GetExtensions().SelectMany(x => x.Features).Distinct());
            }

            return _allUnorderedFeatureInfos;
        }

        private static string GetSourceFeatureNameForType(Type type, string extensionId)
        {
            foreach (OrchardFeatureAttribute featureAttribute in type.GetTypeInfo().GetCustomAttributes(typeof(OrchardFeatureAttribute), false))
            {
                return featureAttribute.FeatureName;
            }
            return extensionId;
        }
    }
}
