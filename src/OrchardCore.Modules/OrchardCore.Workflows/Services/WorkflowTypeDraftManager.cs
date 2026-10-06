using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using YesSql;
using ISession = YesSql.ISession;
using static OrchardCore.Workflows.WorkflowDesignerConstants;

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

    internal readonly IStringLocalizer S;

    public WorkflowTypeDraftManager(
        ISession session,
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowManager workflowManager,
        IActivityLibrary activityLibrary,
        IActivityIdGenerator activityIdGenerator,
        IHttpContextAccessor httpContextAccessor,
        IClock clock,
        IStringLocalizer<WorkflowTypeDraftManager> stringLocalizer)
    {
        _session = session;
        _workflowTypeStore = workflowTypeStore;
        _workflowManager = workflowManager;
        _activityLibrary = activityLibrary;
        _activityIdGenerator = activityIdGenerator;
        _httpContextAccessor = httpContextAccessor;
        _clock = clock;
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
    public Task<WorkflowTypeDraftResult> AddActivityAsync(string workflowTypeId, int expectedRevision, string activityName, int x, int y)
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

            return Task.FromResult(new ChangeOutcome());
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
            draft.Activities = version.Activities.Select(activity => activity.Clone()).ToList();
            draft.Transitions = version.Transitions.Select(transition => transition.Clone()).ToList();

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
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkflowDesignIssue>> ValidateAsync(WorkflowTypeDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        return Task.FromResult<IReadOnlyList<WorkflowDesignIssue>>(Validate(draft.Activities, draft.Transitions));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkflowDesignIssue>> ValidateAsync(WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        return Task.FromResult<IReadOnlyList<WorkflowDesignIssue>>(Validate(workflowType.Activities, workflowType.Transitions));
    }

    private List<WorkflowDesignIssue> Validate(IList<ActivityRecord> activities, IList<Transition> transitions)
    {
        var issues = new List<WorkflowDesignIssue>();
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

        var duplicates = validTransitions
            .GroupBy(transition => (transition.SourceActivityId, transition.SourceOutcomeName))
            .Where(group => group.Count() > 1);

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
        var outcomes = await activityContext.Activity.GetPossibleOutcomesAsync(workflowContext, activityContext);

        return outcomes.Select(outcome => outcome.Name).ToHashSet(StringComparer.Ordinal);
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
