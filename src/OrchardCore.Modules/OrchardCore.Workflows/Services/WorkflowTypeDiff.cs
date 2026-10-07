using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Compares two definitions of a workflow type, for example two versions, or a version and the draft.
/// </summary>
public static class WorkflowTypeDiff
{
    /// <summary>
    /// Returns what changed from <paramref name="from"/> to <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The earlier definition.</param>
    /// <param name="to">The later definition.</param>
    public static WorkflowTypeChanges Compare(WorkflowType from, WorkflowType to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        var fromActivities = from.Activities.ToDictionary(activity => activity.ActivityId);
        var toActivities = to.Activities.ToDictionary(activity => activity.ActivityId);
        var common = toActivities.Values.Where(activity => fromActivities.ContainsKey(activity.ActivityId)).ToList();

        var changed = common
            .Where(activity => !IsSameActivity(fromActivities[activity.ActivityId], activity))
            .Select(activity => activity.ActivityId)
            .ToList();

        var moved = common
            .Where(activity =>
            {
                var previous = fromActivities[activity.ActivityId];

                return (previous.X != activity.X || previous.Y != activity.Y) && !changed.Contains(activity.ActivityId);
            })
            .Select(activity => activity.ActivityId)
            .ToList();

        var fromTransitions = from.Transitions.Select(WorkflowDesignIssue.GetTransitionKey).ToHashSet();
        var toTransitions = to.Transitions.Select(WorkflowDesignIssue.GetTransitionKey).ToHashSet();

        var settings = new List<string>();
        AddIfChanged(settings, nameof(WorkflowType.Name), from.Name, to.Name);
        AddIfChanged(settings, nameof(WorkflowType.IsSingleton), from.IsSingleton, to.IsSingleton);
        AddIfChanged(settings, nameof(WorkflowType.LockTimeout), from.LockTimeout, to.LockTimeout);
        AddIfChanged(settings, nameof(WorkflowType.LockExpiration), from.LockExpiration, to.LockExpiration);
        AddIfChanged(settings, nameof(WorkflowType.DeleteFinishedWorkflows), from.DeleteFinishedWorkflows, to.DeleteFinishedWorkflows);
        AddIfChanged(settings, nameof(WorkflowType.IsActivity), from.IsActivity, to.IsActivity);

        if (!JsonNode.DeepEquals(JsonSerializer.SerializeToNode(from.Variables, JOptions.Default), JsonSerializer.SerializeToNode(to.Variables, JOptions.Default)))
        {
            settings.Add(nameof(WorkflowType.Variables));
        }

        return new WorkflowTypeChanges
        {
            AddedActivityIds = to.Activities.Where(activity => !fromActivities.ContainsKey(activity.ActivityId)).Select(activity => activity.ActivityId).ToList(),
            RemovedActivityIds = from.Activities.Where(activity => !toActivities.ContainsKey(activity.ActivityId)).Select(activity => activity.ActivityId).ToList(),
            ChangedActivityIds = changed,
            MovedActivityIds = moved,
            AddedTransitionKeys = toTransitions.Where(key => !fromTransitions.Contains(key)).ToList(),
            RemovedTransitionKeys = fromTransitions.Where(key => !toTransitions.Contains(key)).ToList(),
            ChangedSettings = settings,
        };
    }

    private static bool IsSameActivity(ActivityRecord previous, ActivityRecord current)
        => previous.Name == current.Name &&
           previous.IsStart == current.IsStart &&
           JsonNode.DeepEquals(previous.Properties ?? [], current.Properties ?? []);

    private static void AddIfChanged<T>(List<string> settings, string name, T previous, T current)
    {
        if (!EqualityComparer<T>.Default.Equals(previous, current))
        {
            settings.Add(name);
        }
    }
}
