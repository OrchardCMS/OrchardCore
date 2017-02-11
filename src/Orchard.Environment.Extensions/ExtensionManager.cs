using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Features.Attributes;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Environment.Extensions.Utility;

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

        private IDictionary<string, ExtensionEntry> _extensions;
        private IDictionary<string, FeatureEntry> _features;

        private ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _featureDependencies
            = new ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>>();

        private ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _dependentFeatures
            = new ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>>();

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

        private bool _isInitialized = false;
        private static object InitializationSyncLock = new object();

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
            EnsureInitialized();

            ExtensionEntry extension;
            if (_extensions.TryGetValue(extensionId, out extension))
            {
                return extension.ExtensionInfo;
            }

            return new NotFoundExtensionInfo(extensionId);
        }

        public IEnumerable<IExtensionInfo> GetExtensions()
        {
            EnsureInitialized();

            return _extensions.Values.Select(ex => ex.ExtensionInfo);
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


        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            EnsureInitialized();

            ExtensionEntry extension;
            if (_extensions.TryGetValue(extensionInfo.Id, out extension))
            {
                return Task.FromResult(extension);
            }

            return Task.FromResult<ExtensionEntry>(null);
        }

        public Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync()
        {
            EnsureInitialized();

            return Task.FromResult<IEnumerable<FeatureEntry>>(_features.Values);
        }

        public Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(string[] featureIdsToLoad)
        {
            EnsureInitialized();

            var loadedFeatures = _features.Values.Where(f => featureIdsToLoad.Contains(f.FeatureInfo.Id));

            return Task.FromResult<IEnumerable<FeatureEntry>>(loadedFeatures);
        }



        public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId)
        {
            EnsureInitialized();

            return _featureDependencies.GetOrAdd(featureId, (key) => new Lazy<IEnumerable<IFeatureInfo>>(() =>
            {
                if (!_features.ContainsKey(key))
                {
                    return Enumerable.Empty<IFeatureInfo>();
                }

                var feature = _features[key].FeatureInfo;

                var dependencies = new HashSet<IFeatureInfo>() { feature };
                var stack = new Stack<IFeatureInfo[]>();

                var unorderedFeatures = GetAllUnorderedFeatures().ToArray();
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
            })).Value;
        }

        public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId)
        {
            EnsureInitialized();

            return _dependentFeatures.GetOrAdd(featureId, (key) => new Lazy<IEnumerable<IFeatureInfo>>(() =>
            {
                if (!_features.ContainsKey(key))
                {
                    return Enumerable.Empty<IFeatureInfo>();
                }

                var feature = _features[key].FeatureInfo;
                if (feature == null)
                {
                    return Enumerable.Empty<IFeatureInfo>();
                }

                var unorderedFeatures = GetAllUnorderedFeatures().ToArray();

                return
                    GetDependentFeatures(feature, unorderedFeatures);
            })).Value;
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

        private IList<IFeatureInfo> Order(IEnumerable<IFeatureInfo> featuresToOrder)
        {
            return featuresToOrder
                .OrderBy(x => x.Id)
                .OrderByDependenciesAndPriorities(
                    HasDependency,
                    GetPriority)
                .ToList();
        }

        private bool HasDependency(IFeatureInfo f1, IFeatureInfo f2)
        {
            return _extensionDependencyStrategies.Any(s => s.HasDependency(f1, f2));
        }

        private int GetPriority(IFeatureInfo feature)
        {
            return _extensionPriorityStrategies.Sum(s => s.GetPriority(feature));
        }

        private IList<IFeatureInfo> GetAllUnorderedFeatures()
        {
            if (_allUnorderedFeatureInfos == null)
            {
                _allUnorderedFeatureInfos = _features.Values.Select(x => x.FeatureInfo).ToList();
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

        private void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            lock (InitializationSyncLock)
            {
                if (_isInitialized)
                {
                    return;
                }

                var extensions = HarvestExtensions();

                var loadedExtensions =
                    new ConcurrentDictionary<string, ExtensionEntry>();

                Parallel.ForEach(extensions, (extension) =>
                {
                    if (extension.Exists)
                    {
                        var entry = _extensionLoader.Load(extension);

                        if (entry.IsError && L.IsEnabled(LogLevel.Warning))
                        {
                            L.LogError("No suitable loader found for extension \"{0}\"", extension.Id);
                        }

                        loadedExtensions.TryAdd(extension.Id, entry);
                    }
                });

                var loadedFeatures =
                    new Dictionary<string, FeatureEntry>();

                foreach (var loadedExtension in loadedExtensions)
                {
                    var extension = loadedExtension.Value;

                    var extensionTypes = extension
                        .ExportedTypes
                        .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract);

                    foreach (var feature in extension.ExtensionInfo.Features)
                    {
                        var featureTypes = new List<Type>();

                        // Search for all types from the extensions that are not assigned to a different
                        // feature.
                        foreach (var type in extensionTypes)
                        {
                            string sourceFeature = GetSourceFeatureNameForType(type, extension.ExtensionInfo.Id);

                            if (sourceFeature == feature.Id)
                            {
                                featureTypes.Add(type);
                                _typeFeatureProvider.TryAdd(type, feature);
                            }
                        }

                        // Search in other extensions for types that are assigned to this feature.
                        var otherExtensionInfos = extensions.Where(x => x.Id != extension.ExtensionInfo.Id);

                        foreach (var otherExtensionInfo in otherExtensionInfos)
                        {
                            var otherExtension = loadedExtensions[otherExtensionInfo.Id];
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

                        loadedFeatures.Add(feature.Id, new CompiledFeatureEntry(feature, featureTypes));
                    }
                };

                _extensions = loadedExtensions.ToDictionary(a => a.Key, b => b.Value);
                _features = loadedFeatures.ToDictionary(a => a.Key, b => b.Value);
                _isInitialized = _extensions != null;
            }
        }

        public IEnumerable<IExtensionInfo> HarvestExtensions()
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

            return extensionsById.Values;
        }
    }
}