using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Extensions.Utility;
using OrchardCore.Modules;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable LoopCanBeConvertedToQuery

namespace OrchardCore.Environment.Extensions
{
    public class ExtensionManager : IExtensionManager
    {
        private readonly IApplicationContext _applicationContext;

        private readonly IExtensionDependencyStrategy[] _extensionDependencyStrategies;
        private readonly IExtensionPriorityStrategy[] _extensionPriorityStrategies;
        private readonly IFeaturesProvider _featuresProvider;

        private FrozenDictionary<string, ExtensionEntry> _extensions;
        private List<IExtensionInfo> _extensionsInfos;
        private Dictionary<string, IFeatureInfo> _features;
        private IFeatureInfo[] _featureInfos;

        private readonly ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _featureDependencies = new();
        private readonly ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _dependentFeatures = new();

        private bool _isInitialized;
        private readonly object _synLock = new();

        public ExtensionManager(
            IApplicationContext applicationContext,
            IEnumerable<IExtensionDependencyStrategy> extensionDependencyStrategies,
            IEnumerable<IExtensionPriorityStrategy> extensionPriorityStrategies,
            IFeaturesProvider featuresProvider,
            ILogger<ExtensionManager> logger)
        {
            _applicationContext = applicationContext;
            _extensionDependencyStrategies = extensionDependencyStrategies as IExtensionDependencyStrategy[] ?? extensionDependencyStrategies.ToArray();
            _extensionPriorityStrategies = extensionPriorityStrategies as IExtensionPriorityStrategy[] ?? extensionPriorityStrategies.ToArray();
            _featuresProvider = featuresProvider;
            L = logger;
        }

        public ILogger L { get; set; }

        public IExtensionInfo GetExtension(string extensionId)
        {
            EnsureInitialized();

            if (!string.IsNullOrEmpty(extensionId) && _extensions.TryGetValue(extensionId, out var extension))
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

            var allDependencyIds = new HashSet<string>(featureIdsToLoad
                .SelectMany(GetFeatureDependencies)
                .Select(x => x.Id));

            foreach (var featureInfo in _featureInfos)
            {
                if (allDependencyIds.Contains(featureInfo.Id))
                {
                    yield return featureInfo;
                }
            }
        }

        public IEnumerable<Type> GetExportedExtensionTypes(IExtensionInfo extensionInfo)
        {
            EnsureInitialized();

            if (_extensions.TryGetValue(extensionInfo.Id, out var extension))
            {
                return extension.ExportedTypes;
            }

            return [];
        }

        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            EnsureInitialized();

            _extensions.TryGetValue(extensionInfo.Id, out var extension);

            return Task.FromResult(extension);
        }

        public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync()
        {
            EnsureInitialized();
            
            return Task.FromResult<IEnumerable<IFeatureInfo>>(_features.Values);
        }

        public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(string[] featureIdsToLoad)
        {
            EnsureInitialized();

            var features = new HashSet<string>(GetFeatures(featureIdsToLoad).Select(f => f.Id));

            var loadedFeatures = _features.Values
                .Where(f => features.Contains(f.Id));

            return Task.FromResult<IEnumerable<IFeatureInfo>>(loadedFeatures);
        }

        public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId)
        {
            EnsureInitialized();

            return _featureDependencies.GetOrAdd(featureId, (key) => new Lazy<IEnumerable<IFeatureInfo>>(() =>
            {
                if (!_features.TryGetValue(key, out var entry))
                {
                    return [];
                }

                return GetFeatureDependencies(entry, _featureInfos);
            })).Value;
        }

        public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId)
        {
            EnsureInitialized();

            return _dependentFeatures.GetOrAdd(featureId, (key) => new Lazy<IEnumerable<IFeatureInfo>>(() =>
            {
                if (!_features.TryGetValue(key, out var entry))
                {
                    return [];
                }

                return GetDependentFeatures(entry, _featureInfos);
            })).Value;
        }

        private IEnumerable<IFeatureInfo> GetFeatureDependencies(
            IFeatureInfo feature,
            IFeatureInfo[] features)
        {
            var dependencyIds = new HashSet<string> { feature.Id };
            var stack = new Stack<List<IFeatureInfo>>();

            stack.Push(GetFeatureDependenciesFunc(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next)
                {
                    if (!dependencyIds.Contains(dependency.Id))
                    {
                        dependencyIds.Add(dependency.Id);
                        stack.Push(GetFeatureDependenciesFunc(dependency, features));
                    }
                }
            }

            // Preserve the underlying order of feature infos.
            foreach (var featureInfo in _featureInfos)
            {
                if (dependencyIds.Contains(featureInfo.Id))
                {
                    yield return featureInfo;
                }
            }
        }

        private IEnumerable<IFeatureInfo> GetDependentFeatures(
            IFeatureInfo feature,
            IFeatureInfo[] features)
        {
            var dependencyIds = new HashSet<string> { feature.Id };
            var stack = new Stack<List<IFeatureInfo>>();

            stack.Push(GetDependentFeaturesFunc(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next)
                {
                    if (!dependencyIds.Contains(dependency.Id))
                    {
                        dependencyIds.Add(dependency.Id);
                        stack.Push(GetDependentFeaturesFunc(dependency, features));
                    }
                }
            }

            // Preserve the underlying order of feature infos.
            foreach (var featureInfo in _featureInfos)
            {
                if (dependencyIds.Contains(featureInfo.Id))
                {
                    yield return featureInfo;
                }
            }
        }

        public IEnumerable<IFeatureInfo> GetFeatures()
        {
            EnsureInitialized();

            return _featureInfos;
        }

        private static List<IFeatureInfo> GetDependentFeaturesFunc(IFeatureInfo currentFeature, IFeatureInfo[] features)
        {
            var list = new List<IFeatureInfo>();
            foreach (var f in features)
            {
                foreach (var dependencyId in f.Dependencies)
                {
                    if (dependencyId == currentFeature.Id)
                    {
                        list.Add(f);
                        break;
                    }
                }
            }

            return list;
        }

        private static List<IFeatureInfo> GetFeatureDependenciesFunc(IFeatureInfo currentFeature, IFeatureInfo[] features)
        {
            var list = new List<IFeatureInfo>();
            foreach (var f in features)
            {
                foreach (var dependencyId in currentFeature.Dependencies)
                {
                    if (dependencyId == f.Id)
                    {
                        list.Add(f);
                        break;
                    }
                }
            }

            return list;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            lock (_synLock)
            {
                if (_isInitialized)
                {
                    return;
                }

                var modules = _applicationContext.Application.Modules;
                var loadedExtensions = new ConcurrentDictionary<string, ExtensionEntry>();

                // Load all extensions in parallel
                Parallel.ForEach(modules, (module, cancellationToken) =>
                {
                    if (!module.ModuleInfo.Exists)
                    {
                        return;
                    }

                    var manifestInfo = new ManifestInfo(module.ModuleInfo);
                    var extensionInfo = new ExtensionInfo(module.SubPath, manifestInfo, (mi, ei) =>
                    {
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

                var loadedFeatures = new Dictionary<string, IFeatureInfo>();

                foreach (var loadedExtension in loadedExtensions)
                {
                    var extension = loadedExtension.Value;

                    foreach (var feature in extension.ExtensionInfo.Features)
                    {
                        loadedFeatures.Add(feature.Id, feature);
                    }
                }

                // Feature infos and entries are ordered by priority and dependencies.
                _featureInfos = Order(loadedFeatures.Values);
                _features = _featureInfos.ToDictionary(f => f.Id, f => loadedFeatures[f.Id]);

                // Extensions are also ordered according to the weight of their first features.
                _extensionsInfos = _featureInfos
                    .Where(f => f.Id == f.Extension.Features.First().Id)
                    .Select(f => f.Extension)
                    .ToList();

                _extensions = _extensionsInfos.ToFrozenDictionary(e => e.Id, e => loadedExtensions[e.Id]);

                _isInitialized = true;
            }
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
            foreach (var s in _extensionDependencyStrategies)
            {
                if (s.HasDependency(f1, f2))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetPriority(IFeatureInfo feature)
        {
            var sum = 0;
            foreach (var strategy in _extensionPriorityStrategies)
            {
                sum += strategy.GetPriority(feature);
            }

            return sum;
        }
    }
}
