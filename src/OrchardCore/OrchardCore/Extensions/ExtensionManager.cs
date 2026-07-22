using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable LoopCanBeConvertedToQuery

namespace OrchardCore.Environment.Extensions;

public sealed class ExtensionManager : IExtensionManager
{
    private readonly IServiceProvider _serviceProvider;

    private FrozenDictionary<string, ExtensionEntry> _extensions;
    private IExtensionInfo[] _extensionsInfos;
    private FrozenDictionary<string, IFeatureInfo> _features;
    private IFeatureInfo[] _featureInfos;

    private readonly ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _featureDependencies = new();
    private readonly ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _dependentFeatures = new();

    private bool _isInitialized;
    private readonly object _synLock = new();

    public ExtensionManager(
        IServiceProvider serviceProvider,
        ILogger<ExtensionManager> logger)
    {
        _serviceProvider = serviceProvider;
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

    public IEnumerable<IFeatureInfo> GetFeatures()
    {
        EnsureInitialized();

        return _featureInfos;
    }

    public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad)
        => GetFeatures((IEnumerable<string>)featureIdsToLoad);

    public IEnumerable<IFeatureInfo> GetFeatures(IEnumerable<string> featureIdsToLoad)
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

    public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
    {
        EnsureInitialized();

        _extensions.TryGetValue(extensionInfo.Id, out var extension);

        return Task.FromResult(extension);
    }

    public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync()
    {
        EnsureInitialized();

        // Must return the features ordered by dependencies.
        return Task.FromResult<IEnumerable<IFeatureInfo>>(_featureInfos);
    }

    public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(string[] featureIdsToLoad)
        => LoadFeaturesAsync((IEnumerable<string>)featureIdsToLoad);

    public Task<IEnumerable<IFeatureInfo>> LoadFeaturesAsync(IEnumerable<string> featureIdsToLoad)
    {
        EnsureInitialized();

        var features = new HashSet<string>(GetFeatures(featureIdsToLoad).Select(f => f.Id));

        // Must return the features ordered by dependencies.
        var loadedFeatures = _featureInfos
            .Where(f => features.Contains(f.Id));

        return Task.FromResult(loadedFeatures);
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

            var applicationContext = _serviceProvider.GetRequiredService<IApplicationContext>();
            var typeFeatureProvider = _serviceProvider.GetRequiredService<ITypeFeatureProvider>();
            var featuresProviders = _serviceProvider.GetServices<IFeaturesProvider>();

            var extensionDependencyStrategies = _serviceProvider.GetServices<IExtensionDependencyStrategy>();
            var extensionPriorityStrategies = _serviceProvider.GetServices<IExtensionPriorityStrategy>();

            var modules = applicationContext.Application.Modules;
            var loadedExtensions = new ConcurrentDictionary<string, ExtensionEntry>();

            // Load all extensions in parallel.
            Parallel.ForEach(modules, (module, cancellationToken) =>
            {
                if (!module.ModuleInfo.Exists)
                {
                    return;
                }

                var manifestInfo = new ManifestInfo(module.ModuleInfo);
                var extensionInfo = new ExtensionInfo(module.SubPath, manifestInfo, (manifestInfo, extensionInfo) =>
                {
                    return featuresProviders.SelectMany(p => p.GetFeatures(extensionInfo, manifestInfo)).ToArray();
                });

                var entry = new ExtensionEntry
                {
                    ExtensionInfo = extensionInfo,
                    Assembly = module.Assembly,
                    ExportedTypes = module.Assembly.ExportedTypes,
                };

                loadedExtensions.TryAdd(module.Name, entry);
            });

            // Get all types from all extension and add them to the type feature provider.
            foreach (var loadedExtension in loadedExtensions)
            {
                var extension = loadedExtension.Value;

                foreach (var exportedType in extension.ExportedTypes.Where(IsComponentType))
                {
                    if (!SkipExtensionFeatureRegistration(exportedType))
                    {
                        var sourceFeature = GetSourceFeatureNameForType(exportedType, extension.ExtensionInfo.Id);

                        var feature = extension.ExtensionInfo.Features.FirstOrDefault(f => f.Id == sourceFeature);

                        if (feature != null)
                        {
                            typeFeatureProvider.TryAdd(exportedType, feature);

                            continue;
                        }

                        // Type has no specific feature, add it to all features.
                        foreach (var curFeature in extension.ExtensionInfo.Features)
                        {
                            typeFeatureProvider.TryAdd(exportedType, curFeature);
                        }
                    }
                }
            }

            // Feature infos and entries are ordered by priority and dependencies.
            _featureInfos = Order(
                loadedExtensions.SelectMany(extension => extension.Value.ExtensionInfo.Features),
                extensionDependencyStrategies as IExtensionDependencyStrategy[] ?? extensionDependencyStrategies.ToArray(),
                extensionPriorityStrategies as IExtensionPriorityStrategy[] ?? extensionPriorityStrategies.ToArray());
            _features = _featureInfos.ToFrozenDictionary(feature => feature.Id, feature => feature);

            // Extensions are also ordered according to the weight of their first features.
            _extensionsInfos = _featureInfos
                .Where(feature => feature.Id == feature.Extension.Features.First().Id)
                .Select(feature => feature.Extension)
                .ToArray();

            _extensions = _extensionsInfos.ToFrozenDictionary(extension => extension.Id, extension => loadedExtensions[extension.Id]);

            if (L.IsEnabled(LogLevel.Trace))
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Feature load order:");

                var featureSet = new HashSet<string>(_featureInfos.Select(f => f.Id));

                for (var i = 0; i < _featureInfos.Length; i++)
                {
                    var f = _featureInfos[i];

                    sb.Append($"  [{i + 1,4}] {f.Id}");

                    if (f.Priority != 0)
                    {
                        sb.Append($"  (priority: {f.Priority})");
                    }

                    var edges = new List<string>();

                    foreach (var dep in f.Dependencies)
                    {
                        if (featureSet.Contains(dep))
                        {
                            edges.Add($"depends on '{dep}'");
                        }
                    }

                    foreach (var after in f.After)
                    {
                        if (featureSet.Contains(after))
                        {
                            edges.Add($"after '{after}'");
                        }
                    }

                    foreach (var before in f.Before)
                    {
                        if (featureSet.Contains(before))
                        {
                            edges.Add($"before '{before}'");
                        }
                    }

                    if (edges.Count > 0)
                    {
                        sb.Append($"  [{string.Join(", ", edges)}]");
                    }

                    sb.AppendLine();
                }

                L.LogTrace("{FeatureGraph}", sb.ToString());
            }

            _isInitialized = true;
        }
    }

    private static IEnumerable<IFeatureInfo> GetFeatureDependencies(
        IFeatureInfo feature,
        IFeatureInfo[] featureInfos)
    {
        var dependencyIds = new HashSet<string> { feature.Id };
        var stack = new Stack<List<IFeatureInfo>>();

        stack.Push(GetFeatureDependenciesFunc(feature, featureInfos));

        while (stack.Count > 0)
        {
            var next = stack.Pop();
            foreach (var dependency in next)
            {
                if (dependencyIds.Add(dependency.Id))
                {
                    stack.Push(GetFeatureDependenciesFunc(dependency, featureInfos));
                }
            }
        }

        // Preserve the underlying order of feature infos.
        foreach (var featureInfo in featureInfos)
        {
            if (dependencyIds.Contains(featureInfo.Id))
            {
                yield return featureInfo;
            }
        }
    }

    private static IEnumerable<IFeatureInfo> GetDependentFeatures(
        IFeatureInfo feature,
        IFeatureInfo[] featureInfos)
    {
        var dependencyIds = new HashSet<string> { feature.Id };
        var stack = new Stack<List<IFeatureInfo>>();

        stack.Push(GetDependentFeaturesFunc(feature, featureInfos));

        while (stack.Count > 0)
        {
            var next = stack.Pop();
            foreach (var dependency in next)
            {
                if (dependencyIds.Add(dependency.Id))
                {
                    stack.Push(GetDependentFeaturesFunc(dependency, featureInfos));
                }
            }
        }

        // Preserve the underlying order of feature infos.
        foreach (var featureInfo in featureInfos)
        {
            if (dependencyIds.Contains(featureInfo.Id))
            {
                yield return featureInfo;
            }
        }
    }

    private static List<IFeatureInfo> GetDependentFeaturesFunc(IFeatureInfo currentFeature, IFeatureInfo[] features)
    {
        var list = new List<IFeatureInfo>();
        foreach (var feature in features)
        {
            foreach (var dependencyId in feature.Dependencies)
            {
                if (dependencyId == currentFeature.Id)
                {
                    list.Add(feature);
                    break;
                }
            }
        }

        return list;
    }

    private static List<IFeatureInfo> GetFeatureDependenciesFunc(IFeatureInfo currentFeature, IFeatureInfo[] features)
    {
        var list = new List<IFeatureInfo>();
        foreach (var feature in features)
        {
            foreach (var dependencyId in currentFeature.Dependencies)
            {
                if (dependencyId == feature.Id)
                {
                    list.Add(feature);
                    break;
                }
            }
        }

        return list;
    }

    private static IFeatureInfo[] Order(IEnumerable<IFeatureInfo> featuresToOrder, IExtensionDependencyStrategy[] extensionDependencyStrategies, IExtensionPriorityStrategy[] extensionPriorityStrategies)
    {
        var features = featuresToOrder.OrderBy(x => x.Id).ToArray();

        var featureById = features.ToDictionary(feature => feature.Id, feature => feature);
        var edges = featureById.Keys.ToDictionary(id => id, _ => new HashSet<string>());
        var indegrees = featureById.Keys.ToDictionary(id => id, _ => 0);

        foreach (var observer in features)
        {
            foreach (var subject in features)
            {
                if (ReferenceEquals(observer, subject))
                {
                    continue;
                }

                if (HasDependency(observer, subject, extensionDependencyStrategies) && edges[subject.Id].Add(observer.Id))
                {
                    indegrees[observer.Id]++;
                }
            }
        }

        // Apply ordering constraints and simultaneously record the Before/After adjacency used
        // for priority propagation. Only Before/After hints (not structural dependency edges)
        // participate in propagation: they express "I want to sit adjacent to this feature",
        // whereas Dependencies express a load-order requirement that should not drag unrelated
        // features into a priority band.
        // After:  observer.After = ["X"] means X -> observer.
        // Before: observer.Before = ["X"] means observer -> X.
        // In both cases, remove any conflicting reverse edge first.
        var orderingHintNeighbors = featureById.Keys.ToDictionary(id => id, _ => new HashSet<string>());

        foreach (var observer in features)
        {
            var observerId = observer.Id;

            foreach (var afterId in observer.After)
            {
                if (featureById.ContainsKey(afterId))
                {
                    orderingHintNeighbors[observerId].Add(afterId);
                    orderingHintNeighbors[afterId].Add(observerId);
                }

                ApplyOrderingConstraint(
                    constrainedId: afterId,
                    reverseFromId: observerId,
                    reverseToId: afterId,
                    forwardFromId: afterId,
                    forwardToId: observerId);
            }

            foreach (var beforeId in observer.Before)
            {
                if (featureById.ContainsKey(beforeId))
                {
                    orderingHintNeighbors[observerId].Add(beforeId);
                    orderingHintNeighbors[beforeId].Add(observerId);
                }

                ApplyOrderingConstraint(
                    constrainedId: beforeId,
                    reverseFromId: beforeId,
                    reverseToId: observerId,
                    forwardFromId: observerId,
                    forwardToId: beforeId);
            }
        }

        void ApplyOrderingConstraint(string constrainedId, string reverseFromId, string reverseToId, string forwardFromId, string forwardToId)
        {
            if (!featureById.ContainsKey(constrainedId))
            {
                return;
            }

            if (edges[reverseFromId].Remove(reverseToId))
            {
                indegrees[reverseToId]--;
            }

            if (edges[forwardFromId].Add(forwardToId))
            {
                indegrees[forwardToId]++;
            }
        }

        var basePriorities = featureById.Keys.ToDictionary(
            id => id,
            id => GetPriority(featureById[id], extensionPriorityStrategies));

        var effectivePriorities = new Dictionary<string, int>(basePriorities);
        var propagated = new HashSet<string>();
        var bfsQueue = new Queue<(string Id, int Priority)>();

        // Seed the BFS with all features that carry an explicit non-default priority.
        // Sort by priority value so that the most extreme priority wins when two sources
        // are equidistant from the same default-priority feature.
        foreach (var id in basePriorities.Keys
            .Where(id => basePriorities[id] != 0)
            .OrderBy(id => basePriorities[id]))
        {
            propagated.Add(id);
            bfsQueue.Enqueue((id, basePriorities[id]));
        }

        while (bfsQueue.Count > 0)
        {
            var (currentId, priority) = bfsQueue.Dequeue();

            foreach (var neighborId in orderingHintNeighbors[currentId])
            {
                // Do not propagate into a neighbor that has its own explicit non-default priority.
                if (basePriorities[neighborId] != 0)
                {
                    continue;
                }

                if (propagated.Add(neighborId))
                {
                    effectivePriorities[neighborId] = priority;
                    bfsQueue.Enqueue((neighborId, priority));
                }
            }
        }

        var queueComparer = Comparer<string>.Create((left, right) =>
        {
            if (left == right)
            {
                return 0;
            }

            var priorityComparison = effectivePriorities[left].CompareTo(effectivePriorities[right]);

            return priorityComparison != 0 ? priorityComparison : string.CompareOrdinal(left, right);
        });

        var queue = new SortedSet<string>(queueComparer);

        foreach (var feature in features)
        {
            if (indegrees[feature.Id] == 0)
            {
                queue.Add(feature.Id);
            }
        }

        var orderedIds = new List<string>(features.Length);

        while (queue.Count > 0)
        {
            var nextId = queue.Min;
            queue.Remove(nextId);
            orderedIds.Add(nextId);

            foreach (var dependentId in edges[nextId])
            {
                indegrees[dependentId]--;

                if (indegrees[dependentId] == 0)
                {
                    queue.Add(dependentId);
                }
            }
        }

        if (orderedIds.Count != features.Length)
        {
            foreach (var remainingFeature in features
                .Where(feature => !orderedIds.Contains(feature.Id))
                .OrderBy(feature => effectivePriorities[feature.Id])
                .ThenBy(feature => feature.Id, StringComparer.Ordinal))
            {
                orderedIds.Add(remainingFeature.Id);
            }
        }

        return orderedIds.Select(id => featureById[id]).ToArray();
    }

    private static bool HasDependency(IFeatureInfo observer, IFeatureInfo subject, IExtensionDependencyStrategy[] extensionDependencyStrategies)
    {
        foreach (var extensionDependencyStrategy in extensionDependencyStrategies)
        {
            if (extensionDependencyStrategy.HasDependency(observer, subject))
            {
                return true;
            }
        }

        return false;
    }

    private static int GetPriority(IFeatureInfo feature, IExtensionPriorityStrategy[] extensionPriorityStrategies)
    {
        var sum = 0;
        foreach (var extensionPriorityStrategy in extensionPriorityStrategies)
        {
            sum += extensionPriorityStrategy.GetPriority(feature);
        }

        return sum;
    }

    private static string GetSourceFeatureNameForType(Type type, string extensionId)
    {
        var attribute = type.GetCustomAttributes<FeatureAttribute>(false).FirstOrDefault();

        return attribute?.FeatureName ?? extensionId;
    }

    private static bool IsComponentType(Type type)
    {
        return type.IsClass && !type.IsAbstract && type.IsPublic;
    }

    private static bool SkipExtensionFeatureRegistration(Type type)
    {
        return FeatureTypeDiscoveryAttribute.GetFeatureTypeDiscoveryForType(type)?.SkipExtension ?? false;
    }
}
