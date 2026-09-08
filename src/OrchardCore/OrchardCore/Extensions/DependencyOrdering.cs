using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions;

internal static class DependencyOrdering
{
    public static IFeatureInfo[] Order(
        IEnumerable<IFeatureInfo> featuresToOrder,
        IExtensionDependencyStrategy[] extensionDependencyStrategies,
        IExtensionPriorityStrategy[] extensionPriorityStrategies,
        out string[] unresolvedFeatureIds,
        out string[] violatedConstraints)
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

        unresolvedFeatureIds = [];

        if (orderedIds.Count != features.Length)
        {
            var orderedIdSet = orderedIds.ToHashSet(StringComparer.Ordinal);

            var remainingFeatures = features
                .Where(feature => !orderedIdSet.Contains(feature.Id))
                .OrderBy(feature => effectivePriorities[feature.Id])
                .ThenBy(feature => feature.Id, StringComparer.Ordinal)
                .ToArray();

            unresolvedFeatureIds = remainingFeatures
                .Select(feature => feature.Id)
                .ToArray();

            foreach (var remainingFeature in remainingFeatures)
            {
                orderedIds.Add(remainingFeature.Id);
            }
        }

        bool HasExplicitOrderingConstraint(IFeatureInfo observer, IFeatureInfo subject)
        {
            var observerId = observer.Id;
            var subjectId = subject.Id;

            return observer.Before.Contains(subjectId)
                || observer.After.Contains(subjectId)
                || subject.Before.Contains(observerId)
                || subject.After.Contains(observerId);
        }

        var positions = orderedIds
            .Select((id, index) => (id, index))
            .ToDictionary(x => x.id, x => x.index, StringComparer.Ordinal);

        var violations = new HashSet<string>(StringComparer.Ordinal);

        foreach (var observer in features)
        {
            var observerPosition = positions[observer.Id];

            foreach (var subject in features)
            {
                if (ReferenceEquals(observer, subject))
                {
                    continue;
                }

                if (HasDependency(observer, subject, extensionDependencyStrategies)
                    && !HasExplicitOrderingConstraint(observer, subject)
                    && positions[subject.Id] >= observerPosition)
                {
                    violations.Add($"dependency: '{observer.Id}' depends on '{subject.Id}'");
                }
            }

            foreach (var afterId in observer.After)
            {
                if (positions.TryGetValue(afterId, out var afterPosition) && afterPosition >= observerPosition)
                {
                    violations.Add($"after: '{observer.Id}' after '{afterId}'");
                }
            }

            foreach (var beforeId in observer.Before)
            {
                if (positions.TryGetValue(beforeId, out var beforePosition) && observerPosition >= beforePosition)
                {
                    violations.Add($"before: '{observer.Id}' before '{beforeId}'");
                }
            }
        }

        violatedConstraints = violations.OrderBy(x => x, StringComparer.Ordinal).ToArray();

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
}
