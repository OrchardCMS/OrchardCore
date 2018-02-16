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

        private IDictionary<string, ExtensionEntry> _extensions;
        private IEnumerable<IExtensionInfo> _extensionsInfos;
        private IDictionary<string, FeatureEntry> _features;
        private IFeatureInfo[] _featureInfos;

        private ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _featureDependencies
            = new ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>>();

        private ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _dependentFeatures
            = new ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>>();

        private static Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]> GetDependentFeaturesFunc =
            new Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]>(
                (currentFeature, fs) => fs
                    .Where(f => f.Dependencies.Any(dep => dep == currentFeature.Id))
                    .ToArray());

        private static Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]> GetFeatureDependenciesFunc =
            new Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]>(
                (currentFeature, fs) => fs
                    .Where(f => currentFeature.Dependencies.Any(dep => dep == f.Id))
                    .ToArray());

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
            if (!String.IsNullOrEmpty(extensionId) && _extensions.TryGetValue(extensionId, out extension))
            {
                return extension.ExtensionInfo;
            }

            return new NotFoundExtensionInfo(extensionId);
        }

        public IEnumerable<IExtensionInfo> GetExtensions()
        {
            EnsureInitialized();

            return _extensionsInfos;
        }

        public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad)
        {
            EnsureInitialized();

            var allDependencies = featureIdsToLoad
                .SelectMany(featureId => GetFeatureDependencies(featureId))
                .Distinct();

            return _featureInfos
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

            var features = GetFeatures(featureIdsToLoad).Select(f => f.Id).ToList();

            var loadedFeatures = _features.Values
                .Where(f => features.Contains(f.FeatureInfo.Id));

            return Task.FromResult(loadedFeatures);
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

                return GetFeatureDependencies(feature, _featureInfos);
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

                return GetDependentFeatures(feature, _featureInfos);
            })).Value;
        }

        private IEnumerable<IFeatureInfo> GetFeatureDependencies(
            IFeatureInfo feature,
            IFeatureInfo[] features)
        {
            var dependencies = new HashSet<IFeatureInfo>() { feature };
            var stack = new Stack<IFeatureInfo[]>();

            stack.Push(GetFeatureDependenciesFunc(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                {
                    dependencies.Add(dependency);
                    stack.Push(GetFeatureDependenciesFunc(dependency, features));
                }
            }

            // Preserve the underlying order of feature infos.
            return _featureInfos.Where(f => dependencies.Any(d => d.Id == f.Id));
        }

        private IEnumerable<IFeatureInfo> GetDependentFeatures(
            IFeatureInfo feature,
            IFeatureInfo[] features)
        {
            var dependencies = new HashSet<IFeatureInfo>() { feature };
            var stack = new Stack<IFeatureInfo[]>();

            stack.Push(GetDependentFeaturesFunc(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                {
                    dependencies.Add(dependency);
                    stack.Push(GetDependentFeaturesFunc(dependency, features));
                }
            }

            // Preserve the underlying order of feature infos.
            return _featureInfos.Where(f => dependencies.Any(d => d.Id == f.Id));
        }

        public IEnumerable<IFeatureInfo> GetFeatures()
        {
            EnsureInitialized();

            return _featureInfos;
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

                var extensions = HarvestExtensions();

                var loadedExtensions = new ConcurrentDictionary<string, ExtensionEntry>();

                // Load all extensions in parallel
                Parallel.ForEach(extensions, (extension) =>
                {
                    if (!extension.Exists)
                    {
                        return;
                    }

                    var entry = _extensionLoader.Load(extension);

                    if (entry.IsError && L.IsEnabled(LogLevel.Warning))
                    {
                        L.LogWarning("No loader found for extension \"{0}\". This might denote a dependency is missing or the extension doesn't have an assembly.", extension.Id);
                    }

                    loadedExtensions.TryAdd(extension.Id, entry);
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

                // Feature infos and entries are ordered by priority and dependencies.
                _featureInfos = Order(loadedFeatures.Values.Select(f => f.FeatureInfo));
                _features = _featureInfos.ToDictionary(f => f.Id, f => loadedFeatures[f.Id]);

                // Extensions are also ordered according to the weight of their first features.
                _extensionsInfos = _featureInfos.Where(f => f.Id == f.Extension.Features.First().Id)
                    .Select(f => f.Extension);

                _extensions = _extensionsInfos.ToDictionary(e => e.Id, e => loadedExtensions[e.Id]);

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
                            var subPath = Path.Combine(searchOption.SearchPath, subDirectory.Name, mc.ManifestFileName);
                            return _hostingEnvironment.ContentRootFileProvider.GetFileInfo(subPath).Exists;
                        });

                    if (manifestConfiguration == null)
                    {
                        continue;
                    }

                    var manifestsubPath = searchOption.SearchPath + '/' + subDirectory.Name;
                    var manifestFilesubPath = manifestsubPath + '/' + manifestConfiguration.ManifestFileName;

                    IConfigurationBuilder configurationBuilder =
                        _manifestProvider.GetManifestConfiguration(new ConfigurationBuilder(), manifestFilesubPath);

                    if (!configurationBuilder.Sources.Any())
                    {
                        continue;
                    }

                    var configurationRoot = configurationBuilder.Build();

                    var manifestInfo = new ManifestInfo(configurationRoot, manifestConfiguration.Type);

                    // Manifest tells you what your loading, subpath is where you are loading it
                    var extensionInfo = _extensionProvider.GetExtensionInfo(manifestInfo, manifestsubPath);

                    extensionSet.Add(extensionInfo);
                }
            }

            return extensionSet;
        }
    }
}
