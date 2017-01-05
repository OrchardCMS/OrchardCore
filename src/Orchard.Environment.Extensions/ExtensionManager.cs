using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Environment.Extensions.Utility;
using Orchard.Environment.Extensions.Features.Attributes;

namespace Orchard.Environment.Extensions
{
    public class ExtensionManager : IExtensionManager
    {
        private readonly ExtensionExpanderOptions _extensionExpanderOptions;
        private readonly ManifestOptions _manifestOptions;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IManifestProvider _manifestProvider;
        private readonly IExtensionProvider _extensionProvider;

        private readonly IExtensionLoader _extensionLoader;
        private readonly IEnumerable<IExtensionDependencyStrategy> _extensionDependencyStrategies;
        private readonly IEnumerable<IExtensionPriorityStrategy> _extensionPriorityStrategies;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        private readonly LazyConcurrentDictionary<string, Task<ExtensionEntry>> _extensions
            = new LazyConcurrentDictionary<string, Task<ExtensionEntry>>();

        private readonly LazyConcurrentDictionary<string, Task<FeatureEntry>> _features
            = new LazyConcurrentDictionary<string, Task<FeatureEntry>>();

        private LazyConcurrentDictionary<string, IEnumerable<IFeatureInfo>> _featureDependencies
            = new LazyConcurrentDictionary<string, IEnumerable<IFeatureInfo>>();

        private LazyConcurrentDictionary<string, IEnumerable<IFeatureInfo>> _dependentFeatures
            = new LazyConcurrentDictionary<string, IEnumerable<IFeatureInfo>>();

        private IDictionary<string, IExtensionInfo> _extensionsById;

        private IList<IFeatureInfo> _allOrderedFeatureInfos;
        private IList<IFeatureInfo> _allUnorderedFeatureInfos;

        private static Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]> GetDependantFeaturesFunc =
            new Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]>(
                (currentFeature, fs) => fs
                    .Where(f =>
                            f.Dependencies.Any(dep => dep == currentFeature.Id)
                           ).OrderBy(x => x.Id).ToArray());

        private static Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]> GetFeatureDependenciesFunc =
            new Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]>(
                (currentFeature, fs) => fs
                    .Where(f =>
                            currentFeature.Dependencies.Any(dep => dep == f.Id)
                           ).OrderByDescending(x => x.Id).ToArray());

        public ExtensionManager(
            IOptions<ExtensionExpanderOptions> extensionExpanderOptionsAccessor,
            IOptions<ManifestOptions> manifestOptionsAccessor,
            IHostingEnvironment hostingEnvironment,
            IEnumerable<IManifestProvider> manifestProviders,

            IEnumerable<IExtensionProvider> extensionProviders,
            IEnumerable<IExtensionLoader> extensionLoaders,
            IEnumerable<IExtensionDependencyStrategy> extensionDependencyStrategies,
            IEnumerable<IExtensionPriorityStrategy> extensionPriorityStrategies,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<ExtensionManager> logger,
            IStringLocalizer<ExtensionManager> localizer)
        {
            _extensionExpanderOptions = extensionExpanderOptionsAccessor.Value;
            _manifestOptions = manifestOptionsAccessor.Value;
            _hostingEnvironment = hostingEnvironment;
            _manifestProvider = new CompositeManifestProvider(manifestProviders);
            _extensionProvider = new CompositeExtensionProvider(extensionProviders);
            _extensionLoader = new CompositeExtensionLoader(extensionLoaders);
            _extensionDependencyStrategies = extensionDependencyStrategies;
            _extensionPriorityStrategies = extensionPriorityStrategies;
            _typeFeatureProvider = typeFeatureProvider;
            L = logger;
            T = localizer;
        }
        public ILogger L { get; set; }
        public IStringLocalizer T { get; set; }

        public IExtensionInfo GetExtension(string extensionId)
        {
            GetExtensions(); // initialize

            if (_extensionsById.ContainsKey(extensionId))
            {
                return _extensionsById[extensionId];
            }

            return new NotFoundExtensionInfo(extensionId);
        }

        public IEnumerable<IExtensionInfo> GetExtensions()
        {
            if (_extensionsById == null)
            {
                 var searchOptions = _extensionExpanderOptions.Options;
 
                 if (searchOptions.Count == 0)
                 {
                     return Enumerable.Empty<IExtensionInfo>();
                 }

                var extensionsById = new Dictionary<string, IExtensionInfo>();

                foreach (var searchOption in searchOptions)
                {
                    foreach (var subDirectory in _hostingEnvironment
                        .ContentRootFileProvider
                        .GetDirectoryContents(searchOption.SearchPath)
                        .Where(x => x.IsDirectory))
                    {
                        var manifestConfiguration = _manifestOptions
                            .ManifestConfigurations
                            .FirstOrDefault(mc =>
                                {
                                    return File.Exists(Path.Combine(subDirectory.PhysicalPath, mc.ManifestFileName));
                                }
                            );

                        if (manifestConfiguration == null)
                        {
                            continue;
                        }

                        var manifestFilesubPath = Path.Combine(searchOption.SearchPath, subDirectory.Name, manifestConfiguration.ManifestFileName);
                        var manifestsubPath = Path.Combine(searchOption.SearchPath, subDirectory.Name);

                        IConfigurationBuilder configurationBuilder =
                            _manifestProvider.GetManifestConfiguration(new ConfigurationBuilder(), manifestFilesubPath);

                        if (!configurationBuilder.Sources.Any())
                        {
                            continue;
                        }

                        var configurationRoot = configurationBuilder.Build();

                        var manifestInfo = new ManifestInfo(configurationRoot, manifestConfiguration.Type);

                        // Manifest tells you what your loading, subpath is where you are loading it
                        var extensionInfo = _extensionProvider
                            .GetExtensionInfo(manifestInfo, manifestsubPath);

                        extensionsById.Add(extensionInfo.Id, extensionInfo);
                    }
                }

                _extensionsById = extensionsById;
            }

            return _extensionsById.Values;
        }

        public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId)
        {
            return _featureDependencies.GetOrAdd(featureId, (key) =>
            {
                var unorderedFeatures = GetAllUnorderedFeatures().ToArray();

                var feature = unorderedFeatures.FirstOrDefault(x => x.Id == key);
                if (feature == null)
                {
                    return Enumerable.Empty<IFeatureInfo>();
                }

                var dependencies = new HashSet<IFeatureInfo>() { feature };
                var stack = new Stack<IFeatureInfo[]>();

                stack.Push(GetFeatureDependenciesFunc(feature, unorderedFeatures));

                while (stack.Count > 0)
                {
                    var next = stack.Pop();
                    foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                    {
                        dependencies.Add(dependency);
                        stack.Push(GetFeatureDependenciesFunc(dependency, unorderedFeatures));
                    }
                }

                return dependencies.Reverse();
            });
        }

        public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId)
        {
            return _dependentFeatures.GetOrAdd(featureId, (key) =>
            {
                var unorderedFeatures = GetAllUnorderedFeatures().ToArray();

                var feature = unorderedFeatures.FirstOrDefault(x => x.Id == key);
                if (feature == null)
                {
                    return Enumerable.Empty<IFeatureInfo>();
                }

                return
                    GetDependentFeatures(feature, unorderedFeatures);
            });
        }

        private IEnumerable<IFeatureInfo> GetDependentFeatures(
            IFeatureInfo feature,
            IFeatureInfo[] features)
        {
            var dependencies = new HashSet<IFeatureInfo>() { feature };
            var stack = new Stack<IFeatureInfo[]>();

            stack.Push(GetDependantFeaturesFunc(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                {
                    dependencies.Add(dependency);
                    stack.Push(GetDependantFeaturesFunc(dependency, features));
                }
            }

            return dependencies;
        }

        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            // Results are cached so that there is no mismatch when loading an assembly twice.
            // Otherwise the same types would not match.
            return _extensions.GetOrAdd(extensionInfo.Id, (key) =>
            {
                var extension = _extensionLoader.Load(extensionInfo);

                if (extension.IsError && L.IsEnabled(LogLevel.Warning))
                {

                    L.LogError("No suitable loader found for extension \"{0}\"", extensionInfo.Id);
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
                _allOrderedFeatureInfos = Order(GetAllUnorderedFeatures());
            }

            return _allOrderedFeatureInfos;
        }

        public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad)
        {
            var allDependencies = featureIdsToLoad
                .SelectMany(featureId => GetFeatureDependencies(featureId))
                .Distinct();

            return GetFeatures()
                .Where(f => allDependencies.Any(d => d.Id == f.Id));
        }

        private IList<IFeatureInfo> Order(IEnumerable<IFeatureInfo> featuresToOrder)
        {
            return featuresToOrder
                .OrderBy(x => x.Id)
                .OrderByDependenciesAndPriorities(
                    HasDependency,
                    GetPriority)
                .ToList();
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

        private bool HasDependency(IFeatureInfo f1, IFeatureInfo f2)
        {
            return _extensionDependencyStrategies.Any(s => s.HasDependency(f1, f2));
        }

        private int GetPriority(IFeatureInfo feature)
        {
            return _extensionPriorityStrategies.Sum(s => s.GetPriority(feature));
        }

        private Task<FeatureEntry> LoadFeatureAsync(IFeatureInfo feature)
        {
            return _features.GetOrAdd(feature.Id, async (key) =>
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

                // Search for all types from the extensions that are not assigned to a different
                // feature.
                foreach (var type in extensionTypes)
                {
                    string sourceFeature = GetSourceFeatureNameForType(type, loadedExtension.ExtensionInfo.Id);

                    if (sourceFeature == feature.Id)
                    {
                        featureTypes.Add(type);
                        _typeFeatureProvider.TryAdd(type, feature);
                    }
                }

                // Search in other extensions for types that are assigned to this feature.
                var otherExtensionInfos = GetExtensions().Where(x => x.Id != loadedExtension.ExtensionInfo.Id);

                foreach (var otherExtensionInfo in otherExtensionInfos)
                {
                    var otherExtension = await LoadExtensionAsync(otherExtensionInfo);
                    foreach (var type in otherExtension.ExportedTypes)
                    {
                        string sourceFeature = GetSourceFeatureNameForType(type, null);

                        if (sourceFeature == feature.Id)
                        {
                            featureTypes.Add(type);
                            _typeFeatureProvider.TryAdd(type, feature);
                        }
                    }
                }

                return new CompiledFeatureEntry(feature, featureTypes);
            });
        }

        private IList<IFeatureInfo> GetAllUnorderedFeatures()
        {
            if (_allUnorderedFeatureInfos == null)
            {
                _allUnorderedFeatureInfos = GetExtensions().SelectMany(x => x.Features).ToList();
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

        // https://blogs.endjin.com/2015/10/using-lazy-and-concurrentdictionary-to-ensure-a-thread-safe-run-once-lazy-loaded-collection/
        private class LazyConcurrentDictionary<TKey, TValue>
        {
            private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _concurrentDictionary;

            public LazyConcurrentDictionary()
            {
                _concurrentDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
            }

            // When you call GetOrAdd the valueFactory is not thread safe, this means two threads could make the same
            // call to underlying components.
            // Loading features and extensions is expensive and should only be done once
            public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
            {
                return _concurrentDictionary
                    .GetOrAdd(
                        key, 
                        k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication))
                    .Value;
            }
        }
    }
}