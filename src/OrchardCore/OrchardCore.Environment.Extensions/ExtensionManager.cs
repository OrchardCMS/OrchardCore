using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Loaders;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Extensions.Utility;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Extensions
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
        private readonly IFeaturesProvider _featuresProvider;

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
            IFeaturesProvider featuresProvider,
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
            _featuresProvider = featuresProvider;
            L = logger;
            T = localizer;
        }
        public ILogger L { get; set; }
        public IStringLocalizer T { get; set; }

        public IExtensionInfo GetExtension(string extensionId)
        {
            EnsureInitialized();

            ExtensionEntry extension;
            if (!String.IsNullOrEmpty(extensionId) && _extensions.TryGetValue(extensionId, out extension))
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

            var orderedFeaturesIds = GetFeatures(featureIdsToLoad).Select(f => f.Id).ToList();

            var loadedFeatures = _features.Values
                .Where(f => orderedFeaturesIds.Contains(f.FeatureInfo.Id))
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

                return GetDependentFeatures(feature, _allOrderedFeatureInfos);
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
            var attribute = type.GetCustomAttributes<FeatureAttribute>(false).FirstOrDefault();

            return attribute?.FeatureName ?? extensionId;
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

                var loadedExtensions = new ConcurrentDictionary<string, ExtensionEntry>();

                var moduleNames = _hostingEnvironment.GetApplication().ModuleNames;

                // Load all extensions in parallel
                Parallel.ForEach(moduleNames, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (name) =>
                {
                    var module = _hostingEnvironment.GetModule(name);

                    var manifestConfiguration = _manifestOptions
                        .ManifestConfigurations
                        .FirstOrDefault(mc =>
                        {
                            return module.ModuleInfo.Type.Equals(mc.Type, StringComparison.OrdinalIgnoreCase);
                        });

                    if (manifestConfiguration == null)
                    {
                        return;
                    }

                    var manifestInfo = new ManifestInfo(module.ModuleInfo);

                    var extensionInfo = new ExtensionInfo(module.SubPath, manifestInfo, (mi, ei) => {
                        return _featuresProvider.GetFeatures(ei, mi);
                    });

                    var entry = new ExtensionEntry
                    {
                        ExtensionInfo = extensionInfo,
                        Assembly = module.Assembly,
                        ExportedTypes = module.Assembly.ExportedTypes
                    };

                    loadedExtensions.TryAdd(module.Name, entry);
                });

                var loadedFeatures = new Dictionary<string, FeatureEntry>();

                // Get all valid types from any extension
                var allTypesByExtension = loadedExtensions.SelectMany(extension =>
                    extension.Value.ExportedTypes.Where(IsComponentType)
                    .Select(type => new
                    {
                        ExtensionEntry = extension.Value,
                        Type = type
                    })).ToArray();

                var typesByFeature = allTypesByExtension
                    .GroupBy(typeByExtension => GetSourceFeatureNameForType(
                        typeByExtension.Type, 
                        typeByExtension.ExtensionEntry.ExtensionInfo.Id))
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(typesByExtension => typesByExtension.Type).ToArray());

                foreach (var loadedExtension in loadedExtensions)
                {
                    var extension = loadedExtension.Value;

                    foreach (var feature in extension.ExtensionInfo.Features)
                    {
                        // Features can have no types
                        if (typesByFeature.TryGetValue(feature.Id, out var featureTypes))
                        {
                            foreach (var type in featureTypes)
                            {
                                _typeFeatureProvider.TryAdd(type, feature);
                            }
                        }
                        else
                        {
                            featureTypes = Array.Empty<Type>();
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

        private bool IsComponentType(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.IsPublic;
        }

        private IFeatureInfo[] Order(IEnumerable<IFeatureInfo> featuresToOrder)
        {
            return featuresToOrder
                .OrderBy(x => x.Id)
                .Distinct()
                .OrderByDependenciesAndPriorities(HasDependency, GetPriority)
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
    }
}
