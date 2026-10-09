using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using static OrchardCore.Workflows.WorkflowDesignerConstants;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Workflows.Services;

/// <inheritdoc />
public sealed class WorkflowTypeDraftManager : IWorkflowTypeDraftManager
{
    // The number of removed activities a draft keeps so that undo can restore them.
    private const int MaxRemovedActivities = 100;

    private readonly ISession _session;
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowManager _workflowManager;
    private readonly IActivityLibrary _activityLibrary;
    private readonly IActivityIdGenerator _activityIdGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClock _clock;
    private readonly IWorkflowDesignerNotifier _notifier;

    internal readonly IStringLocalizer S;

    public WorkflowTypeDraftManager(
        ISession session,
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowManager workflowManager,
        IActivityLibrary activityLibrary,
        IActivityIdGenerator activityIdGenerator,
        IHttpContextAccessor httpContextAccessor,
        IClock clock,
        IWorkflowDesignerNotifier notifier,
        IStringLocalizer<WorkflowTypeDraftManager> stringLocalizer)
    {
        _session = session;
        _workflowTypeStore = workflowTypeStore;
        _workflowManager = workflowManager;
        _activityLibrary = activityLibrary;
        _activityIdGenerator = activityIdGenerator;
        _httpContextAccessor = httpContextAccessor;
        _clock = clock;
        _notifier = notifier;
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public Task<WorkflowTypeDraft> GetAsync(string workflowTypeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(workflowTypeId);

        return _session.Query<WorkflowTypeDraft, WorkflowTypeDraftIndex>(index => index.WorkflowTypeId == workflowTypeId)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<WorkflowTypeDraft> GetOrCreateAsync(WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        var draft = await GetAsync(workflowType.WorkflowTypeId);

        if (draft is not null)
        {
            return draft;
        }

        var now = _clock.UtcNow;

        draft = workflowType.CreateDraft();
        draft.CreatedUtc = now;
        draft.ModifiedUtc = now;

        await _session.SaveAsync(draft, checkConcurrency: true);

        return draft;
    }

    /// <inheritdoc />
    public Task<WorkflowTypeDraftResult> SaveGraphAsync(string workflowTypeId, int expectedRevision, WorkflowGraphUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        return ChangeAsync(workflowTypeId, expectedRevision, (_, draft) =>
        {
            draft.RemovedActivities ??= [];

            foreach (var activityId in update.RestoredActivityIds ?? [])
            {
                var removed = draft.RemovedActivities.LastOrDefault(activity => activity.ActivityId == activityId);

                if (removed is not null && !draft.Activities.Any(activity => activity.ActivityId == activityId))
                {
                    draft.RemovedActivities.Remove(removed);
                    draft.Activities.Add(removed);
                }
            }

            var removedIds = new HashSet<string>(update.RemovedActivityIds ?? [], StringComparer.Ordinal);

            foreach (var activity in draft.Activities.Where(activity => removedIds.Contains(activity.ActivityId)).ToList())
            {
                draft.Activities.Remove(activity);
                draft.RemovedActivities.Add(activity);
            }

            while (draft.RemovedActivities.Count > MaxRemovedActivities)
            {
                draft.RemovedActivities.RemoveAt(0);
            }

            var activities = draft.Activities.ToDictionary(activity => activity.ActivityId, StringComparer.Ordinal);

            foreach (var node in update.Nodes ?? [])
            {
                if (node?.Id is null || !activities.TryGetValue(node.Id, out var activity))
                {
                    continue;
                }

                activity.X = node.X;
                activity.Y = node.Y;
                activity.IsStart = node.IsStart;
            }

            // Transitions to or from activities that don't exist are dropped (the 'InvalidTransition' rule).
            draft.Transitions = (update.Transitions ?? [])
                .Where(transition => transition is not null &&
                    transition.SourceActivityId is not null &&
                    transition.DestinationActivityId is not null &&
                    activities.ContainsKey(transition.SourceActivityId) &&
                    activities.ContainsKey(transition.DestinationActivityId))
                .Select(transition => transition.Clone())
                .ToList();

            return Task.FromResult(new ChangeOutcome());
        });
    }

    /// <inheritdoc />
    public Task<WorkflowTypeDraftResult> AddActivityAsync(string workflowTypeId, int expectedRevision, string activityName, int x, int y, JsonObject properties = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(activityName);

        return ChangeAsync(workflowTypeId, expectedRevision, (_, draft) =>
        {
            var activity = _activityLibrary.InstantiateActivity(activityName);

            if (activity is null)
            {
                return Task.FromResult(ChangeOutcome.Rejected(WorkflowTypeDraftStatus.Invalid));
            }

            var record = new ActivityRecord
            {
                Name = activity.Name,
                X = x,
                Y = y,
                Properties = activity.Properties?.DeepClone().AsObject() ?? [],

                // A workflow can't run without a start activity, so the first event becomes the start activity.
                IsStart = activity.IsEvent() && !draft.Activities.Any(existing => existing.IsStart),
            };

            foreach (var (name, value) in properties ?? [])
            {
                record.Properties[name] = value?.DeepClone();
            }

            record.ActivityId = _activityIdGenerator.GenerateUniqueId(record);
            draft.Activities.Add(record);

            return Task.FromResult(new ChangeOutcome { Activity = record });
        });
    }

    /// <inheritdoc />
    public Task<WorkflowTypeDraftResult> UpdateActivityAsync(string workflowTypeId, int expectedRevision, string activityId, JsonObject properties)
    {
        ArgumentException.ThrowIfNullOrEmpty(activityId);
        ArgumentNullException.ThrowIfNull(properties);

        return ChangeAsync(workflowTypeId, expectedRevision, async (workflowType, draft) =>
        {
            var record = draft.Activities.FirstOrDefault(activity => activity.ActivityId == activityId);

            if (record is null)
            {
                return ChangeOutcome.Rejected(WorkflowTypeDraftStatus.NotFound);
            }

            if (_activityLibrary.GetActivityByName(record.Name) is null)
            {
                return ChangeOutcome.Rejected(WorkflowTypeDraftStatus.Invalid);
            }

            record.Properties = properties.DeepClone().AsObject();

            var outcomes = await GetOutcomeNamesAsync(workflowType, draft, record);
            var removed = draft.Transitions
                .Where(transition => transition.SourceActivityId == activityId && !outcomes.Contains(transition.SourceOutcomeName))
                .ToList();

            foreach (var transition in removed)
            {
                draft.Transitions.Remove(transition);
            }

            return new ChangeOutcome { Activity = record, RemovedTransitions = removed };
        });
    }

    /// <inheritdoc />
    public Task<WorkflowTypeDraftResult> UpdateSettingsAsync(string workflowTypeId, int expectedRevision, WorkflowTypeDraftSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return ChangeAsync(workflowTypeId, expectedRevision, (_, draft) =>
        {
            draft.Name = settings.Name?.Trim();
            draft.IsEnabled = settings.IsEnabled;
            draft.IsSingleton = settings.IsSingleton;
            draft.LockTimeout = settings.LockTimeout;
            draft.LockExpiration = settings.LockExpiration;
            draft.DeleteFinishedWorkflows = settings.DeleteFinishedWorkflows;
            draft.IsActivity = settings.IsActivity;
            draft.BranchingMode = settings.BranchingMode;
            draft.FaultOnScriptErrors = settings.FaultOnScriptErrors;
            draft.RecordActivityData = settings.RecordActivityData;

            return Task.FromResult(new ChangeOutcome());
        });
    }

    /// <inheritdoc />
    public Task<WorkflowTypeDraftResult> UpdateVariablesAsync(string workflowTypeId, int expectedRevision, IList<WorkflowVariableDefinition> variables)
    {
        ArgumentNullException.ThrowIfNull(variables);

        return ChangeAsync(workflowTypeId, expectedRevision, (workflowType, draft) =>
        {
            draft.Variables = variables
                .Select(variable =>
                {
                    var copy = variable.Clone();
                    copy.Name = copy.Name?.Trim();

                    return copy;
                })
                .ToList();

            return Task.FromResult(new ChangeOutcome());
        });
    }

    /// <inheritdoc />
    public Task<WorkflowTypeDraftResult> UpdateOutputBindingsAsync(string workflowTypeId, int expectedRevision, string activityId, IDictionary<string, string> bindings)
    {
        ArgumentException.ThrowIfNullOrEmpty(activityId);

        return ChangeAsync(workflowTypeId, expectedRevision, (workflowType, draft) =>
        {
            var record = draft.Activities.FirstOrDefault(activity => activity.ActivityId == activityId);

            if (record is null)
            {
                return Task.FromResult(ChangeOutcome.Rejected(WorkflowTypeDraftStatus.NotFound));
            }

            record.Properties ??= [];
            record.Properties.SetOutputBindings(bindings);

            return Task.FromResult(new ChangeOutcome { Activity = record });
        });
    }

    /// <inheritdoc />
    public Task<WorkflowTypeDraftResult> RestoreAsync(string workflowTypeId, int expectedRevision, WorkflowTypeVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        return ChangeAsync(workflowTypeId, expectedRevision, (workflowType, draft) =>
        {
            if (version.WorkflowTypeId != workflowType.WorkflowTypeId)
            {
                return Task.FromResult(ChangeOutcome.Rejected(WorkflowTypeDraftStatus.NotFound));
            }

            draft.IsSingleton = version.IsSingleton;
            draft.LockTimeout = version.LockTimeout;
            draft.LockExpiration = version.LockExpiration;
            draft.DeleteFinishedWorkflows = version.DeleteFinishedWorkflows;
            draft.IsActivity = version.IsActivity;
            draft.BranchingMode = version.BranchingMode;
            draft.FaultOnScriptErrors = version.FaultOnScriptErrors;
            draft.RecordActivityData = version.RecordActivityData;
            draft.Activities = version.Activities.Select(activity => activity.Clone()).ToList();
            draft.Transitions = version.Transitions.Select(transition => transition.Clone()).ToList();
            draft.Variables = version.Variables.Select(variable => variable.Clone()).ToList();

            return Task.FromResult(new ChangeOutcome());
        });
    }

    /// <inheritdoc />
    public async Task<WorkflowTypeDraftResult> PublishAsync(string workflowTypeId, int expectedRevision)
    {
        ArgumentException.ThrowIfNullOrEmpty(workflowTypeId);

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        var draft = workflowType is null ? null : await GetAsync(workflowTypeId);

        if (draft is null)
        {
            return new WorkflowTypeDraftResult { Status = WorkflowTypeDraftStatus.NotFound };
        }

        if (draft.Revision != expectedRevision)
        {
            return Conflict(draft);
        }

        var issues = await ValidateAsync(draft);

        if (issues.Any(issue => issue.Severity == WorkflowDesignIssueSeverity.Error))
        {
            return new WorkflowTypeDraftResult
            {
                Status = WorkflowTypeDraftStatus.Invalid,
                Draft = draft,
                Revision = draft.Revision,
                Issues = issues,
            };
        }

        draft.ApplyTo(workflowType);

        await _workflowTypeStore.SaveAsync(workflowType);
        _session.Delete(draft);

        await NotifyAsync(WorkflowTypeChangeKind.Published, workflowTypeId, 0, workflowType.VersionId);

        return new WorkflowTypeDraftResult
        {
            Status = WorkflowTypeDraftStatus.Succeeded,
            Issues = issues,
            WorkflowType = workflowType,
        };
    }

    /// <inheritdoc />
    public async Task DiscardAsync(string workflowTypeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(workflowTypeId);

        var drafts = await _session.Query<WorkflowTypeDraft, WorkflowTypeDraftIndex>(index => index.WorkflowTypeId == workflowTypeId)
            .ListAsync();

        foreach (var draft in drafts)
        {
            _session.Delete(draft);
        }

        if (drafts.Any())
        {
            await NotifyAsync(WorkflowTypeChangeKind.DraftDiscarded, workflowTypeId, 0);
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkflowDesignIssue>> ValidateAsync(WorkflowTypeDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        return Task.FromResult<IReadOnlyList<WorkflowDesignIssue>>(Validate(draft.WorkflowTypeId, draft.BranchingMode, draft.Activities, draft.Transitions, draft.Variables));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkflowDesignIssue>> ValidateAsync(WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        return Task.FromResult<IReadOnlyList<WorkflowDesignIssue>>(Validate(workflowType.WorkflowTypeId, workflowType.BranchingMode, workflowType.Activities, workflowType.Transitions, workflowType.Variables));
    }

    private List<WorkflowDesignIssue> Validate(
        string workflowTypeId,
        WorkflowBranchingMode branchingMode,
        IList<ActivityRecord> activities,
        IList<Transition> transitions,
        IList<WorkflowVariableDefinition> variables)
    {
        var issues = new List<WorkflowDesignIssue>();

        ValidateVariableReferences(activities, variables, issues);
        var activityIds = new HashSet<string>(activities.Select(activity => activity.ActivityId), StringComparer.Ordinal);

        if (activities.Count > 0 && !activities.Any(activity => activity.IsStart))
        {
            issues.Add(new WorkflowDesignIssue
            {
                Severity = WorkflowDesignIssueSeverity.Warning,
                Code = IssueCodes.MissingStartActivity,
                Message = S["This workflow has no start activity, so it never runs. Mark an event as the start activity."],
            });
        }

        foreach (var activity in activities)
        {
            if (_activityLibrary.GetActivityByName(activity.Name) is null)
            {
                issues.Add(new WorkflowDesignIssue
                {
                    Severity = WorkflowDesignIssueSeverity.Warning,
                    Code = IssueCodes.MissingActivity,
                    Message = S["The activity type '{0}' isn't available. Enable the feature that provides it, or remove the activity.", activity.Name],
                    ActivityId = activity.ActivityId,
                });
            }
        }

        ValidateWorkflowExecutions(workflowTypeId, activities, issues);

        var validTransitions = new List<Transition>();

        foreach (var transition in transitions)
        {
            if (!activityIds.Contains(transition.SourceActivityId ?? string.Empty) ||
                !activityIds.Contains(transition.DestinationActivityId ?? string.Empty))
            {
                issues.Add(new WorkflowDesignIssue
                {
                    Severity = WorkflowDesignIssueSeverity.Error,
                    Code = IssueCodes.InvalidTransition,
                    Message = S["A transition of the outcome '{0}' connects an activity that doesn't exist. It's removed when the workflow is saved.", transition.SourceOutcomeName],
                    ActivityId = activityIds.Contains(transition.SourceActivityId ?? string.Empty) ? transition.SourceActivityId : null,
                    TransitionKey = WorkflowDesignIssue.GetTransitionKey(transition),
                });

                continue;
            }

            validTransitions.Add(transition);
        }

        // Every transition of an outcome is followed in the All mode.
        var duplicates = validTransitions
            .GroupBy(transition => (transition.SourceActivityId, transition.SourceOutcomeName))
            .Where(group => branchingMode == WorkflowBranchingMode.FirstOnly && group.Count() > 1);

        foreach (var group in duplicates)
        {
            foreach (var transition in group.Skip(1))
            {
                issues.Add(new WorkflowDesignIssue
                {
                    Severity = WorkflowDesignIssueSeverity.Warning,
                    Code = IssueCodes.DuplicateOutcomeTransition,
                    Message = S["The outcome '{0}' has more than one transition. Only the first one is followed.", group.Key.SourceOutcomeName],
                    ActivityId = group.Key.SourceActivityId,
                    TransitionKey = WorkflowDesignIssue.GetTransitionKey(transition),
                });
            }
        }

        var startIds = activities.Where(activity => activity.IsStart).Select(activity => activity.ActivityId).ToList();

        // Reachability is only meaningful once there is a start activity; otherwise 'MissingStartActivity' covers it.
        if (startIds.Count > 0)
        {
            var reachable = new HashSet<string>(startIds, StringComparer.Ordinal);
            var pending = new Queue<string>(startIds);
            var outgoing = validTransitions.ToLookup(transition => transition.SourceActivityId, StringComparer.Ordinal);

            while (pending.TryDequeue(out var activityId))
            {
                foreach (var transition in outgoing[activityId])
                {
                    if (reachable.Add(transition.DestinationActivityId))
                    {
                        pending.Enqueue(transition.DestinationActivityId);
                    }
                }
            }

            foreach (var activity in activities.Where(activity => !reachable.Contains(activity.ActivityId)))
            {
                issues.Add(new WorkflowDesignIssue
                {
                    Severity = WorkflowDesignIssueSeverity.Warning,
                    Code = IssueCodes.UnreachableActivity,
                    Message = S["This activity can't be reached from a start activity, so it never runs."],
                    ActivityId = activity.ActivityId,
                });
            }
        }

        return issues;
    }

    // Execute Workflow tasks that don't say which workflow to run, or that run the workflow they belong to.
    private void ValidateWorkflowExecutions(string workflowTypeId, IList<ActivityRecord> activities, List<WorkflowDesignIssue> issues)
    {
        foreach (var activity in activities.Where(activity => activity.Name == nameof(ExecuteWorkflowTask)))
        {
            var target = activity.Properties?[nameof(ExecuteWorkflowTask.WorkflowTypeId)] is JsonValue value && value.TryGetValue<string>(out var id) ? id : null;

            if (string.IsNullOrEmpty(target))
            {
                issues.Add(new WorkflowDesignIssue
                {
                    Severity = WorkflowDesignIssueSeverity.Warning,
                    Code = IssueCodes.MissingWorkflowToExecute,
                    Message = S["Select the workflow this activity runs."],
                    ActivityId = activity.ActivityId,
                });
            }
            else if (target == workflowTypeId)
            {
                issues.Add(new WorkflowDesignIssue
                {
                    Severity = WorkflowDesignIssueSeverity.Warning,
                    Code = IssueCodes.RecursiveWorkflowExecution,
                    Message = S["This activity runs the workflow it belongs to. Make sure the workflow stops running itself: workflows can run each other {0} levels deep at most.", WorkflowManager.MaxChildWorkflowDepth],
                    ActivityId = activity.ActivityId,
                });
            }
        }
    }

    // Set Variable activities and output bindings that name an undeclared variable, and bindings whose output values
    // may not convert to their variable's type.
    private void ValidateVariableReferences(IList<ActivityRecord> activities, IList<WorkflowVariableDefinition> variables, List<WorkflowDesignIssue> issues)
    {
        var declared = (variables ?? [])
            .Where(variable => !string.IsNullOrEmpty(variable?.Name))
            .GroupBy(variable => variable.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var activity in activities)
        {
            if (activity.Name == nameof(SetVariableTask) &&
                activity.Properties?[nameof(SetVariableTask.VariableName)] is JsonValue nameValue &&
                nameValue.TryGetValue<string>(out var variableName) &&
                !string.IsNullOrWhiteSpace(variableName) &&
                !declared.ContainsKey(variableName))
            {
                issues.Add(new WorkflowDesignIssue
                {
                    Severity = WorkflowDesignIssueSeverity.Warning,
                    Code = IssueCodes.UndeclaredVariable,
                    Message = S["'{0}' isn't a declared variable: the value is stored as a workflow property, without a type.", variableName],
                    ActivityId = activity.ActivityId,
                });
            }

            var bindings = activity.Properties.GetOutputBindings();

            if (bindings.Count == 0)
            {
                continue;
            }

            var outputs = (_activityLibrary.GetActivityByName(activity.Name) as IActivityOutputs)?.GetOutputs().ToDictionary(output => output.Name) ?? [];

            foreach (var (outputName, boundVariable) in bindings)
            {
                if (!declared.TryGetValue(boundVariable, out var variable))
                {
                    issues.Add(new WorkflowDesignIssue
                    {
                        Severity = WorkflowDesignIssueSeverity.Warning,
                        Code = IssueCodes.UndeclaredVariable,
                        Message = S["The output '{0}' is bound to '{1}', which isn't a declared variable.", outputName, boundVariable],
                        ActivityId = activity.ActivityId,
                    });
                }
                else if (outputs.TryGetValue(outputName, out var output) && !IsAssignable(output.TypeName, variable.TypeName))
                {
                    issues.Add(new WorkflowDesignIssue
                    {
                        Severity = WorkflowDesignIssueSeverity.Warning,
                        Code = IssueCodes.OutputTypeMismatch,
                        Message = S["The output '{0}' ({1}) is bound to '{2}' ({3}): its values may not convert, which faults the workflow.", outputName, output.TypeName, variable.Name, variable.TypeName],
                        ActivityId = activity.ActivityId,
                    });
                }
            }
        }
    }

    // Any value converts to text, and nothing is known of 'any' outputs.
    private static bool IsAssignable(string outputType, string variableType)
        => string.Equals(outputType, variableType, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(outputType, "any", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(variableType, "any", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(variableType, "string", StringComparison.OrdinalIgnoreCase);

    private async Task<WorkflowTypeDraftResult> ChangeAsync(
        string workflowTypeId,
        int expectedRevision,
        Func<WorkflowType, WorkflowTypeDraft, Task<ChangeOutcome>> change)
    {
        ArgumentException.ThrowIfNullOrEmpty(workflowTypeId);

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return new WorkflowTypeDraftResult { Status = WorkflowTypeDraftStatus.NotFound };
        }

        var draft = await GetAsync(workflowTypeId);

        // Without a draft, the caller saw the live definition, which is revision 0.
        if ((draft?.Revision ?? 0) != expectedRevision)
        {
            return draft is null
                ? new WorkflowTypeDraftResult { Status = WorkflowTypeDraftStatus.Conflict, Revision = 0 }
                : Conflict(draft);
        }

        // The draft is only persisted once a change succeeds, so a rejected change never leaves an empty draft.
        if (draft is null)
        {
            draft = workflowType.CreateDraft();
            draft.CreatedUtc = _clock.UtcNow;
        }

        var outcome = await change(workflowType, draft);

        if (outcome.Status != WorkflowTypeDraftStatus.Succeeded)
        {
            return new WorkflowTypeDraftResult
            {
                Status = outcome.Status,
                Draft = draft.Id == 0 ? null : draft,
                Revision = draft.Revision,
            };
        }

        var user = _httpContextAccessor.HttpContext?.User;

        draft.Revision++;
        draft.ModifiedUtc = _clock.UtcNow;
        draft.ModifiedByUserId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        draft.ModifiedByUserName = user?.Identity?.Name;

        await _session.SaveAsync(draft, checkConcurrency: true);

        try
        {
            await _session.SaveChangesAsync();
        }
        catch (ConcurrencyException)
        {
            // Another request saved the same draft in between.
            return new WorkflowTypeDraftResult { Status = WorkflowTypeDraftStatus.Conflict, Revision = draft.Revision - 1 };
        }

        await NotifyAsync(WorkflowTypeChangeKind.DraftChanged, workflowTypeId, draft.Revision);

        return new WorkflowTypeDraftResult
        {
            Status = WorkflowTypeDraftStatus.Succeeded,
            Draft = draft,
            Revision = draft.Revision,
            Activity = outcome.Activity,
            RemovedTransitions = outcome.RemovedTransitions,
            Issues = await ValidateAsync(draft),
        };
    }

    private async Task<HashSet<string>> GetOutcomeNamesAsync(WorkflowType workflowType, WorkflowTypeDraft draft, ActivityRecord record)
    {
        var transientType = draft.ToTransientWorkflowType(workflowType);
        var workflow = _workflowManager.NewWorkflow(transientType);

        using var workflowContext = await _workflowManager.CreateWorkflowExecutionContextAsync(transientType, workflow);
        var activityContext = await _workflowManager.CreateActivityExecutionContextAsync(record, record.Properties);
        var outcomes = await WorkflowDesignerModelBuilder.GetOutcomesAsync(workflowContext, activityContext);

        return outcomes.Select(outcome => outcome.Name).ToHashSet(StringComparer.Ordinal);
    }

    private Task NotifyAsync(WorkflowTypeChangeKind kind, string workflowTypeId, int revision, string versionId = null)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        return _notifier.WorkflowTypeChangedAsync(new WorkflowTypeChange
        {
            Kind = kind,
            WorkflowTypeId = workflowTypeId,
            Revision = revision,
            VersionId = versionId,
            UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = user?.Identity?.Name,
        });
    }

    private static WorkflowTypeDraftResult Conflict(WorkflowTypeDraft draft)
        => new()
        {
            Status = WorkflowTypeDraftStatus.Conflict,
            Draft = draft,
            Revision = draft.Revision,
            ModifiedByUserName = draft.ModifiedByUserName,
            ModifiedUtc = draft.ModifiedUtc,
        };

    private sealed class ChangeOutcome
    {
        public WorkflowTypeDraftStatus Status { get; init; } = WorkflowTypeDraftStatus.Succeeded;

        public ActivityRecord Activity { get; init; }

        public IReadOnlyList<Transition> RemovedTransitions { get; init; } = [];

        public static ChangeOutcome Rejected(WorkflowTypeDraftStatus status) => new() { Status = status };
    }
}
