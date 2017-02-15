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

        private IFeatureInfo[] _allOrderedFeatureInfos;

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

        public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad)
        {
            EnsureInitialized();

            var allDependencies = featureIdsToLoad
                .SelectMany(featureId => GetFeatureDependencies(featureId))
                .Distinct();

            return _allOrderedFeatureInfos
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
            var orderedFeaturesIds = GetFeatures().Select(f => f.Id).ToList();

            var loadedFeatures = _features.Values
                .OrderBy(f => orderedFeaturesIds.IndexOf(f.FeatureInfo.Id));

            return Task.FromResult<IEnumerable<FeatureEntry>>(loadedFeatures);
        }

        public Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(string[] featureIdsToLoad)
        {
            EnsureInitialized();

            var orderedFeaturesIds = GetFeatures().Select(f => f.Id).ToList();

            var loadedFeatures = _features.Values
                .Where(f => featureIdsToLoad.Contains(f.FeatureInfo.Id))
                .OrderBy(f => orderedFeaturesIds.IndexOf(f.FeatureInfo.Id));

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
                
                stack.Push(GetFeatureDependenciesFunc(feature, _allOrderedFeatureInfos));

                while (stack.Count > 0)
                {
                    var next = stack.Pop();
                    foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                    {
                        dependencies.Add(dependency);
                        stack.Push(GetFeatureDependenciesFunc(dependency, _allOrderedFeatureInfos));
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

                return
                    GetDependentFeatures(feature, _allOrderedFeatureInfos);
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

        public IEnumerable<IFeatureInfo> GetFeatures()
        {
            EnsureInitialized();

            return _allOrderedFeatureInfos;
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
                    if (!extension.Exists)
                    {
                        return;
                    }

                    var entry = _extensionLoader.Load(extension);

                    if (entry.IsError && L.IsEnabled(LogLevel.Warning))
                    {
                        L.LogError("No suitable loader found for extension \"{0}\"", extension.Id);
                    }

                    loadedExtensions.TryAdd(extension.Id, entry);
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
                        var featureTypes = new HashSet<Type>();

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

                _extensions = loadedExtensions;
                // Could we get rid of _allOrderedFeatureInfos and just have _features?
                _features = loadedFeatures;
                _allOrderedFeatureInfos = Order(loadedFeatures.Values.Select(x => x.FeatureInfo));
                _isInitialized = true;
            }
        }

        private IFeatureInfo[] Order(IEnumerable<IFeatureInfo> featuresToOrder)
        {
            return featuresToOrder
                .OrderBy(x => x.Id)
                .Distinct()
                .OrderByDependenciesAndPriorities(
                    HasDependency,
                    GetPriority)
                .ToArray();
        }

        private bool HasDependency(IFeatureInfo f1, IFeatureInfo f2)
        {
            return _extensionDependencyStrategies.Any(s => s.HasDependency(f1, f2));
        }

        private int GetPriority(IFeatureInfo feature)
        {
            return _extensionPriorityStrategies.Sum(s => s.GetPriority(feature));
        }

        private ISet<IExtensionInfo> HarvestExtensions()
        {
            var searchOptions = _extensionExpanderOptions.Options;

            var extensionSet = new HashSet<IExtensionInfo>();

            if (searchOptions.Count == 0)
            {
                return extensionSet;
            }

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

                    var manifestsubPath = Path.Combine(searchOption.SearchPath, subDirectory.Name);
                    var manifestFilesubPath = Path.Combine(manifestsubPath, manifestConfiguration.ManifestFileName);

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

                    extensionSet.Add(extensionInfo);
                }
            }

            return extensionSet;
        }
    }
}