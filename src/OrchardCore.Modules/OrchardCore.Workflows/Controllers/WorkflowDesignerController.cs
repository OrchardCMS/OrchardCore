using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Workflows.Events;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Variables;
using OrchardCore.Workflows.ViewModels;
using YesSql;
using YesSql.Services;
using ISession = YesSql.ISession;

namespace OrchardCore.Workflows.Controllers;

/// <summary>
/// The JSON endpoints of the workflow designer. Changes are saved into the workflow type draft; only
/// <see cref="Publish"/> changes the live workflow type.
/// </summary>
[Admin("Workflows/Types/{workflowTypeId}/Designer/{action}", "WorkflowDesigner{action}")]
public sealed class WorkflowDesignerController : Controller
{
    private const string FragmentViewName = "Fragment";
    private const string SettingsPartialName = "WorkflowDesignerSettings";

    // The number of journal records the instance viewer shows.
    private const int JournalRecordCount = 500;

    // The statuses of the instances that haven't finished, halted ones included.
    private static readonly WorkflowStatus[] s_unfinishedStatuses =
    [
        WorkflowStatus.Idle,
        WorkflowStatus.Starting,
        WorkflowStatus.Resuming,
        WorkflowStatus.Executing,
        WorkflowStatus.Halted,
    ];

    private readonly IAuthorizationService _authorizationService;
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowTypeVersionStore _versionStore;
    private readonly IWorkflowStore _workflowStore;
    private readonly IWorkflowTypeDraftManager _draftManager;
    private readonly WorkflowDesignerModelBuilder _modelBuilder;
    private readonly IWorkflowManager _workflowManager;
    private readonly IActivityDisplayManager _activityDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly ISession _session;
    private readonly IClock _clock;
    private readonly IWorkflowVariableTypeProvider _variableTypes;
    private readonly IWorkflowExecutionJournal _journal;
    private readonly WorkflowVariableValidator _variableValidator;
    private readonly IDistributedLock _distributedLock;

    internal readonly IStringLocalizer S;

    public WorkflowDesignerController(
        IAuthorizationService authorizationService,
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowTypeVersionStore versionStore,
        IWorkflowStore workflowStore,
        IWorkflowTypeDraftManager draftManager,
        WorkflowDesignerModelBuilder modelBuilder,
        IWorkflowManager workflowManager,
        IActivityDisplayManager activityDisplayManager,
        IUpdateModelAccessor updateModelAccessor,
        ISession session,
        IClock clock,
        IWorkflowVariableTypeProvider variableTypes,
        WorkflowVariableValidator variableValidator,
        IWorkflowExecutionJournal journal,
        IDistributedLock distributedLock,
        IStringLocalizer<WorkflowDesignerController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _workflowTypeStore = workflowTypeStore;
        _versionStore = versionStore;
        _workflowStore = workflowStore;
        _draftManager = draftManager;
        _modelBuilder = modelBuilder;
        _workflowManager = workflowManager;
        _activityDisplayManager = activityDisplayManager;
        _updateModelAccessor = updateModelAccessor;
        _session = session;
        _clock = clock;
        _variableTypes = variableTypes;
        _journal = journal;
        _variableValidator = variableValidator;
        _distributedLock = distributedLock;
        S = stringLocalizer;
    }

    [HttpGet]
    public async Task<IActionResult> Definition(long workflowTypeId)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var draft = await _draftManager.GetAsync(workflowType.WorkflowTypeId);
        var source = draft?.ToTransientWorkflowType(workflowType) ?? workflowType;
        var issues = await _draftManager.ValidateAsync(source);

        var runningInstanceCount = await _session.QueryIndex<WorkflowIndex>(index =>
                index.WorkflowTypeId == workflowType.WorkflowTypeId &&
                index.WorkflowStatus.IsIn(s_unfinishedStatuses))
            .CountAsync();

        var model = await BuildDefinitionAsync(workflowType, source);

        model.Revision = draft?.Revision ?? await _draftManager.GetLiveRevisionAsync(workflowType);
        model.HasDraft = draft is not null;
        model.DraftModifiedBy = draft?.ModifiedByUserName;
        model.DraftModifiedByUserId = draft?.ModifiedByUserId;
        model.DraftModifiedUtc = draft?.ModifiedUtc;
        model.Issues = issues;
        model.RunningInstanceCount = runningInstanceCount;
        model.Run = RunOf(workflowType);

        return Ok(model);
    }

    /// <summary>
    /// Runs the published version of a workflow type that starts with <see cref="StartedByWorkflowEvent"/>, with the
    /// values of its input variables, as the Execute Workflow task does. It requires the permission to execute
    /// workflows.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Run(long workflowTypeId, [FromBody] WorkflowDesignerRunRequest request)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ExecuteWorkflows))
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var run = RunOf(workflowType);

        if (!run.IsEnabled)
        {
            return Problem(title: S["The workflow is disabled. Enable it to run it."], statusCode: StatusCodes.Status400BadRequest);
        }

        if (run.Mode != WorkflowDesignerRun.InputsMode)
        {
            return Problem(title: S["Only a published workflow that starts with Started by Workflow runs with inputs."], statusCode: StatusCodes.Status400BadRequest);
        }

        // As the other ways to start an instance do, a singleton is checked and started under the lock of its type.
        (var locker, var locked) = await _distributedLock.TryAcquireWorkflowTypeLockAsync(workflowType);

        if (!locked)
        {
            return Problem(title: S["This workflow runs one instance at a time, and one is running."], statusCode: StatusCodes.Status409Conflict);
        }

        await using var acquiredLock = locker;

        if (workflowType.IsSingleton && await _workflowStore.HasHaltedInstanceAsync(workflowType.WorkflowTypeId))
        {
            return Problem(title: S["This workflow runs one instance at a time, and one is running."], statusCode: StatusCodes.Status409Conflict);
        }

        var input = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var invalid = new List<string>();

        foreach (var variable in run.Inputs)
        {
            var value = request?.Inputs?.FirstOrDefault(entry => string.Equals(entry.Key, variable.Name, StringComparison.OrdinalIgnoreCase)).Value;

            if (value is null)
            {
                continue;
            }

            if (_variableTypes.Get(variable.TypeName) is { } type && !type.TryCoerce(value, out _))
            {
                invalid.Add(variable.Name);

                continue;
            }

            input[variable.Name] = value;
        }

        if (invalid.Count > 0)
        {
            return Problem(title: S["These inputs don't have a value of their type: {0}.", string.Join(", ", invalid)], statusCode: StatusCodes.Status400BadRequest);
        }

        var start = workflowType.Activities.First(activity => activity.ActivityId == run.ActivityId);
        var workflowContext = await _workflowManager.StartWorkflowAsync(workflowType, start, input);
        var workflow = workflowContext.Workflow;
        var exists = workflow.Id != 0 && await _workflowStore.GetAsync(workflow.Id) is not null;

        return Ok(new WorkflowDesignerRunResult
        {
            InstanceId = exists ? workflow.Id : null,
            InstanceUrl = exists ? InstanceUrl(workflow.Id) : null,
            Status = workflowContext.Status.ToString(),
            FaultMessage = workflow.FaultMessage,
            Outputs = workflowContext.Variables.GetOutputs().ToDictionary(
                output => output.Key,
                output => SerializeValue(output.Value),
                StringComparer.OrdinalIgnoreCase),
        });
    }

    /// <summary>
    /// The newest instance of a workflow type, which the Run dialog looks for once the request it sent to an HTTP
    /// Request event answered. It requires the permission to execute workflows.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LatestInstance(long workflowTypeId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ExecuteWorkflows))
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var latest = await _session.Query<Workflow, WorkflowIndex>(index => index.WorkflowTypeId == workflowType.WorkflowTypeId)
            .OrderByDescending(index => index.DocumentId)
            .FirstOrDefaultAsync();

        return Ok(new WorkflowDesignerRunResult
        {
            InstanceId = latest?.Id,
            InstanceUrl = latest is null ? null : InstanceUrl(latest.Id),
            Status = latest?.Status.ToString(),
            FaultMessage = latest?.FaultMessage,
        });
    }

    /// <summary>
    /// The data of an activity's execution, from a record of an instance's journal: what the activity evaluated, set
    /// and changed (see <see cref="WorkflowType.RecordActivityData"/>). The viewer loads it when the record is opened.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> JournalData(long workflowTypeId, long instanceId, int sequence)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var (_, workflow) = await GetInstanceAsync(workflowTypeId, instanceId);

        if (workflow is null)
        {
            return this.ApiNotFoundProblem();
        }

        var record = await _journal.GetAsync(workflow.WorkflowId, sequence);

        return record?.Data is null ? this.ApiNotFoundProblem() : Ok(record.Data);
    }

    /// <summary>
    /// The graph of the read-only instance viewer: the version the instance runs on (never the draft), with the
    /// activities the instance waits on.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Instance(long workflowTypeId, long instanceId)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var (workflowType, workflow) = await GetInstanceAsync(workflowTypeId, instanceId);

        if (workflow is null)
        {
            return this.ApiNotFoundProblem();
        }

        var definition = await _versionStore.GetWorkflowTypeAsync(workflowType, workflow.WorkflowTypeVersionId);
        var version = string.IsNullOrEmpty(workflow.WorkflowTypeVersionId) ? null : await _versionStore.GetAsync(workflow.WorkflowTypeVersionId);
        var journal = await _journal.ListAsync(workflow.WorkflowId, JournalRecordCount);

        // The pages of the parent instance and of the child instances the journal names, when they still exist.
        var linkedWorkflowIds = journal
            .Select(record => record.ChildWorkflowId)
            .Append(workflow.ParentWorkflowId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToArray();
        var linkedInstanceUrls = linkedWorkflowIds.Length == 0
            ? []
            : (await _session.QueryIndex<WorkflowIndex>(index => index.WorkflowId.IsIn(linkedWorkflowIds)).ListAsync())
                .ToDictionary(index => index.WorkflowId, index => InstanceUrl(index.DocumentId));

        var journalRecords = journal.Select(WorkflowDesignerJournalRecord.From).ToList();

        foreach (var record in journalRecords.Where(record => record.ChildWorkflowId is not null))
        {
            record.ChildInstanceUrl = linkedInstanceUrls.GetValueOrDefault(record.ChildWorkflowId);
        }

        var model = await BuildDefinitionAsync(workflowType, definition);

        model.Version = WorkflowDesignerVersion.From(version, workflowType);
        model.Instance = new WorkflowDesignerInstance
        {
            Id = workflow.Id,
            WorkflowId = workflow.WorkflowId,
            Status = workflow.Status.ToString(),
            BlockingActivityIds = workflow.BlockingActivities.Select(activity => activity.ActivityId).Distinct().ToList(),
            VariableValues = VariableValuesOf(workflow, definition.Variables),
            FaultMessage = workflow.FaultMessage,
            FaultedActivityId = workflow.Status == WorkflowStatus.Faulted ? workflow.GetFaultedActivityId(journal) : null,
            PendingRetry = workflow.Status == WorkflowStatus.Faulted ? workflow.PendingRetry : null,
            ParentWorkflowId = workflow.ParentWorkflowId,
            ParentInstanceUrl = string.IsNullOrEmpty(workflow.ParentWorkflowId) ? null : linkedInstanceUrls.GetValueOrDefault(workflow.ParentWorkflowId),
            Journal = journalRecords,
            ExecutedActivityCounts = journal
                .GroupBy(record => record.ActivityId)
                .ToDictionary(group => group.Key, group => group.Count()),
            ExecutedTransitionCounts = ExecutedTransitionCounts(journal, definition),
        };

        return Ok(model);
    }

    /// <summary>
    /// Runs a faulted instance again from one of its activities. It requires the permission to execute workflows.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Retry(long workflowTypeId, [FromBody] WorkflowDesignerRetryRequest request)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ExecuteWorkflows))
        {
            return this.ApiForbidProblem();
        }

        if (string.IsNullOrEmpty(request?.ActivityId))
        {
            return this.ApiBadRequestProblem();
        }

        var (_, workflow) = await GetInstanceAsync(workflowTypeId, request.InstanceId);

        if (workflow is null)
        {
            return this.ApiNotFoundProblem();
        }

        if (workflow.Status != WorkflowStatus.Faulted)
        {
            return Problem(title: S["Only a faulted instance can be retried."], statusCode: StatusCodes.Status400BadRequest);
        }

        WorkflowExecutionContext workflowContext;

        try
        {
            workflowContext = await _workflowManager.RetryActivityAsync(workflow, request.ActivityId);
        }
        catch (ArgumentException)
        {
            return Problem(title: S["The instance's definition doesn't have this activity."], statusCode: StatusCodes.Status400BadRequest);
        }

        if (workflowContext is null)
        {
            return Problem(title: S["The instance is running. Try again in a moment."], statusCode: StatusCodes.Status409Conflict);
        }

        return Ok(new
        {
            status = workflowContext.Status.ToString(),
            faultMessage = workflowContext.Workflow.FaultMessage,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Library(long workflowTypeId)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        if (await _workflowTypeStore.GetAsync(workflowTypeId) is null)
        {
            return this.ApiNotFoundProblem();
        }

        return Ok(await _modelBuilder.BuildLibraryAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Save(long workflowTypeId, [FromBody] WorkflowDesignerSaveRequest request)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        if (request is null)
        {
            return this.ApiBadRequestProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var result = await _draftManager.SaveGraphAsync(workflowType.WorkflowTypeId, request.Revision, new WorkflowGraphUpdate
        {
            Nodes = request.Nodes ?? [],
            Transitions = (request.Transitions ?? []).Where(transition => transition is not null).Select(transition => transition.ToTransition()).ToList(),
            RemovedActivityIds = request.RemovedActivityIds ?? [],
            RestoredActivityIds = request.RestoredActivityIds ?? [],
        });

        if (!result.Succeeded)
        {
            return FailureResult(result);
        }

        return Ok(new
        {
            revision = result.Revision,
            issues = result.Issues,
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddActivity(long workflowTypeId, [FromBody] WorkflowDesignerAddActivityRequest request)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        if (string.IsNullOrEmpty(request?.Name) && string.IsNullOrEmpty(request?.Preset))
        {
            return this.ApiBadRequestProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var activityName = request.Name;
        JsonObject properties = null;

        if (!string.IsNullOrEmpty(request.Preset))
        {
            var preset = await _modelBuilder.FindPresetAsync(request.Preset);

            if (preset is null)
            {
                return this.ApiBadRequestProblem();
            }

            activityName = preset.ActivityName;
            properties = preset.Properties;
        }

        var result = await _draftManager.AddActivityAsync(workflowType.WorkflowTypeId, request.Revision, activityName, request.X, request.Y, properties);

        if (!result.Succeeded)
        {
            return FailureResult(result);
        }

        return Ok(new
        {
            revision = result.Revision,
            node = await _modelBuilder.BuildNodeAsync(result.Draft.ToTransientWorkflowType(workflowType), result.Activity.ActivityId),
            issues = result.Issues,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Editor(long workflowTypeId, string activityId)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var (workflowType, activityContext) = await GetActivityAsync(workflowTypeId, activityId);

        if (activityContext is null)
        {
            return this.ApiNotFoundProblem();
        }

        var editor = await _activityDisplayManager.BuildEditorAsync(activityContext.Activity, _updateModelAccessor.ModelUpdater, isNew: false, "", "");
        editor.Metadata.Type = "Activity_Edit";

        return Fragment(new WorkflowDesignerFragmentViewModel
        {
            Shape = editor,
            WorkflowTypeId = workflowType.Id,
            ActivityId = activityId,
        });
    }

    [HttpPost]
    [ActionName(nameof(Editor))]
    public async Task<IActionResult> EditorPost(long workflowTypeId, string activityId, int revision)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var (workflowType, activityContext) = await GetActivityAsync(workflowTypeId, activityId);

        if (activityContext is null)
        {
            return this.ApiNotFoundProblem();
        }

        var activity = activityContext.Activity;
        var editor = await _activityDisplayManager.UpdateEditorAsync(activity, _updateModelAccessor.ModelUpdater, isNew: false, "", "");

        if (!ModelState.IsValid)
        {
            editor.Metadata.Type = "Activity_Edit";

            return Fragment(new WorkflowDesignerFragmentViewModel
            {
                Shape = editor,
                WorkflowTypeId = workflowType.Id,
                ActivityId = activityId,
                Valid = false,
            });
        }

        var result = await _draftManager.UpdateActivityAsync(workflowType.WorkflowTypeId, revision, activityId, activity.Properties);

        if (!result.Succeeded)
        {
            return FailureResult(result);
        }

        return Ok(new
        {
            valid = true,
            revision = result.Revision,
            node = await _modelBuilder.BuildNodeAsync(result.Draft.ToTransientWorkflowType(workflowType), activityId),
            removedTransitions = result.RemovedTransitions.Select(WorkflowDesignerTransition.From).ToList(),
            issues = result.Issues,
        });
    }

    /// <summary>
    /// Replaces the variables of the draft. Invalid declarations are rejected with their
    /// <c>variableErrors</c>.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Variables(long workflowTypeId, [FromBody] WorkflowDesignerVariablesRequest request)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        if (request?.Variables is null || request.Variables.Any(variable => variable is null))
        {
            return this.ApiBadRequestProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        foreach (var variable in request.Variables)
        {
            variable.Name = variable.Name?.Trim();
        }

        var errors = _variableValidator.Validate(request.Variables);

        if (errors.Count > 0)
        {
            return Problem(
                title: S["The variables can't be saved."],
                detail: S["Fix the errors listed in the variable errors first."],
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object> { ["variableErrors"] = errors });
        }

        var result = await _draftManager.UpdateVariablesAsync(workflowType.WorkflowTypeId, request.Revision, request.Variables);

        if (!result.Succeeded)
        {
            return FailureResult(result);
        }

        return Ok(new
        {
            revision = result.Revision,
            variables = result.Draft.Variables,
            issues = result.Issues,
        });
    }

    /// <summary>
    /// Replaces the output bindings of an activity of the draft.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> OutputBindings(long workflowTypeId, [FromBody] WorkflowDesignerOutputBindingsRequest request)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        if (string.IsNullOrEmpty(request?.ActivityId))
        {
            return this.ApiBadRequestProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var result = await _draftManager.UpdateOutputBindingsAsync(workflowType.WorkflowTypeId, request.Revision, request.ActivityId, request.Bindings ?? new Dictionary<string, string>());

        if (!result.Succeeded)
        {
            return FailureResult(result);
        }

        return Ok(new
        {
            revision = result.Revision,
            node = await _modelBuilder.BuildNodeAsync(result.Draft.ToTransientWorkflowType(workflowType), request.ActivityId),
            issues = result.Issues,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Settings(long workflowTypeId)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var draft = await _draftManager.GetAsync(workflowType.WorkflowTypeId);
        var source = draft?.ToTransientWorkflowType(workflowType) ?? workflowType;

        var model = new WorkflowTypePropertiesViewModel
        {
            Id = workflowType.Id,
            Name = source.Name,
            IsEnabled = source.IsEnabled,
        };

        source.CopyExecutionSettingsTo(model);

        return Fragment(new WorkflowDesignerFragmentViewModel
        {
            PartialName = SettingsPartialName,
            PartialModel = model,
            WorkflowTypeId = workflowType.Id,
        });
    }

    [HttpPost]
    [ActionName(nameof(Settings))]
    public async Task<IActionResult> SettingsPost(long workflowTypeId, int revision)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var model = new WorkflowTypePropertiesViewModel { Id = workflowType.Id };

        if (!await TryUpdateModelAsync(model) || !ModelState.IsValid)
        {
            return Fragment(new WorkflowDesignerFragmentViewModel
            {
                PartialName = SettingsPartialName,
                PartialModel = model,
                WorkflowTypeId = workflowType.Id,
                Valid = false,
            });
        }

        var settings = new WorkflowTypeDraftSettings
        {
            Name = model.Name,
            IsEnabled = model.IsEnabled,
        };

        model.CopyExecutionSettingsTo(settings);

        var result = await _draftManager.UpdateSettingsAsync(workflowType.WorkflowTypeId, revision, settings);

        if (!result.Succeeded)
        {
            return FailureResult(result);
        }

        settings.Name = result.Draft.Name;

        return Ok(new
        {
            valid = true,
            revision = result.Revision,
            settings,
            issues = result.Issues,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Publish(long workflowTypeId, [FromBody] WorkflowDesignerRevisionRequest request)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var result = await _draftManager.PublishAsync(workflowType.WorkflowTypeId, request?.Revision ?? 0);

        if (!result.Succeeded)
        {
            return FailureResult(result);
        }

        return Ok(new
        {
            issues = result.Issues,
            publishedUtc = _clock.UtcNow,
            version = await PublishedVersionAsync(result.WorkflowType ?? workflowType),
            revision = await _draftManager.GetLiveRevisionAsync(result.WorkflowType ?? workflowType),
        });
    }

    /// <summary>
    /// The versions of the workflow type, the most recent first, with the number of instances on each, and the
    /// draft if there is one.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Versions(long workflowTypeId)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var versions = await _versionStore.ListAsync(workflowType.WorkflowTypeId);
        var draft = await _draftManager.GetAsync(workflowType.WorkflowTypeId);

        // The versions are few, while their instances can be many: count them rather than loading them.
        var versionModels = new List<WorkflowDesignerVersion>();

        foreach (var version in versions)
        {
            var instanceCount = await _session.QueryIndex<WorkflowIndex>(index => index.WorkflowTypeVersionId == version.VersionId).CountAsync();

            versionModels.Add(WorkflowDesignerVersion.From(version, workflowType, instanceCount));
        }

        return Ok(new
        {
            versions = versionModels,
            draft = draft is null
                ? null
                : new
                {
                    revision = draft.Revision,
                    modifiedUtc = draft.ModifiedUtc,
                    modifiedBy = draft.ModifiedByUserName,
                },
        });
    }

    /// <summary>
    /// A version of the workflow type, as a read-only definition.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Version(long workflowTypeId, string versionId)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        var version = workflowType is null ? null : await GetVersionAsync(workflowType, versionId);

        if (version is null)
        {
            return this.ApiNotFoundProblem();
        }

        return Ok(await ReadOnlyDefinitionAsync(workflowType, ToDefinition(version, workflowType), version));
    }

    /// <summary>
    /// Two definitions of the workflow type and what changed from the first to the second. Each one is a version
    /// id, or <c>draft</c>.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Compare(long workflowTypeId, string from, string to)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var (fromDefinition, fromVersion) = await ResolveDefinitionAsync(workflowType, from);
        var (toDefinition, toVersion) = await ResolveDefinitionAsync(workflowType, to);

        if (fromDefinition is null || toDefinition is null)
        {
            return this.ApiNotFoundProblem();
        }

        return Ok(new
        {
            from = await ReadOnlyDefinitionAsync(workflowType, fromDefinition, fromVersion),
            to = await ReadOnlyDefinitionAsync(workflowType, toDefinition, toVersion),
            changes = WorkflowTypeDiff.Compare(fromDefinition, toDefinition),
        });
    }

    /// <summary>
    /// Copies a version into the draft, so it can be published again as the next version.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Restore(long workflowTypeId, [FromBody] WorkflowDesignerRestoreRequest request)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        var version = workflowType is null ? null : await GetVersionAsync(workflowType, request?.VersionId);

        if (version is null)
        {
            return this.ApiNotFoundProblem();
        }

        var result = await _draftManager.RestoreAsync(workflowType.WorkflowTypeId, request.Revision, version);

        if (!result.Succeeded)
        {
            return FailureResult(result);
        }

        return Ok(new
        {
            revision = result.Revision,
            issues = result.Issues,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Discard(long workflowTypeId)
    {
        if (!await CanManageAsync())
        {
            return this.ApiForbidProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        await _draftManager.DiscardAsync(workflowType.WorkflowTypeId);

        return Ok(new { });
    }

    private static WorkflowTypeDraftSettings SettingsOf(WorkflowType workflowType)
    {
        var settings = new WorkflowTypeDraftSettings
        {
            Name = workflowType.Name,
            IsEnabled = workflowType.IsEnabled,
        };

        workflowType.CopyExecutionSettingsTo(settings);

        return settings;
    }

    private async Task<WorkflowDesignerVersion> PublishedVersionAsync(WorkflowType workflowType)
        => string.IsNullOrEmpty(workflowType.VersionId)
            ? null
            : WorkflowDesignerVersion.From(await _versionStore.GetAsync(workflowType.VersionId), workflowType);

    // A version as a definition, with the name it had then.
    private static WorkflowType ToDefinition(WorkflowTypeVersion version, WorkflowType workflowType)
    {
        var definition = version.ToWorkflowType(workflowType);
        definition.Name = version.Name;

        return definition;
    }

    private async Task<(WorkflowType Definition, WorkflowTypeVersion Version)> ResolveDefinitionAsync(WorkflowType workflowType, string id)
    {
        if (string.Equals(id, "draft", StringComparison.OrdinalIgnoreCase))
        {
            var draft = await _draftManager.GetAsync(workflowType.WorkflowTypeId);

            return (draft?.ToTransientWorkflowType(workflowType), null);
        }

        var version = await GetVersionAsync(workflowType, id);

        return version is null ? (null, null) : (ToDefinition(version, workflowType), version);
    }

    // A version of the workflow type, or null when it doesn't have one with this id.
    private async Task<WorkflowTypeVersion> GetVersionAsync(WorkflowType workflowType, string versionId)
    {
        var version = string.IsNullOrEmpty(versionId) ? null : await _versionStore.GetAsync(versionId);

        return version?.WorkflowTypeId == workflowType.WorkflowTypeId ? version : null;
    }

    // A definition that is only shown: a version, or the draft (when version is null).
    private async Task<WorkflowDesignerDefinition> ReadOnlyDefinitionAsync(WorkflowType workflowType, WorkflowType definition, WorkflowTypeVersion version)
    {
        var model = await BuildDefinitionAsync(workflowType, definition);

        model.HasDraft = version is null;
        model.Version = WorkflowDesignerVersion.From(version, workflowType);

        return model;
    }

    // What every view of a definition of the workflow type shows: its settings, graph and variables, and the values
    // its expressions can use. The definition is the live type, a draft or a version.
    private async Task<WorkflowDesignerDefinition> BuildDefinitionAsync(WorkflowType workflowType, WorkflowType definition)
        => new()
        {
            Id = workflowType.Id,
            WorkflowTypeId = workflowType.WorkflowTypeId,
            Settings = SettingsOf(definition),
            Nodes = await _modelBuilder.BuildNodesAsync(definition),
            Transitions = definition.Transitions.Select(WorkflowDesignerTransition.From).ToList(),
            Variables = [.. definition.Variables ?? []],
            VariableTypes = _modelBuilder.BuildVariableTypes(),
            GlobalValues = _modelBuilder.BuildGlobalValues(),
            PublishedVersion = await PublishedVersionAsync(workflowType),
        };

    // The transitions the journal's outcomes took, as the engine follows them: the first transition of each outcome,
    // or all of them (WorkflowBranchingMode.All).
    private static Dictionary<string, int> ExecutedTransitionCounts(IReadOnlyList<WorkflowExecutionRecord> journal, WorkflowType definition)
    {
        var counts = new Dictionary<string, int>();

        foreach (var record in journal)
        {
            foreach (var outcome in record.Outcomes ?? [])
            {
                var transitions = definition.Transitions.Where(x => x.SourceActivityId == record.ActivityId && x.SourceOutcomeName == outcome);

                if (definition.BranchingMode == WorkflowBranchingMode.FirstOnly)
                {
                    transitions = transitions.Take(1);
                }

                foreach (var transition in transitions)
                {
                    var key = WorkflowDesignIssue.GetTransitionKey(transition);
                    counts[key] = counts.GetValueOrDefault(key) + 1;
                }
            }
        }

        return counts;
    }

    // The stored values of the declared variables. Values live in the workflow properties, which the state stores
    // as JSON.
    private static Dictionary<string, JsonNode> VariableValuesOf(Workflow workflow, IList<WorkflowVariableDefinition> variables)
    {
        var values = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);

        if (variables is null || variables.Count == 0 || workflow.State is null)
        {
            return values;
        }

        var properties = workflow.State
            .FirstOrDefault(property => string.Equals(property.Key, nameof(WorkflowState.Properties), StringComparison.OrdinalIgnoreCase))
            .Value as JsonObject;

        if (properties is null)
        {
            return values;
        }

        foreach (var variable in variables)
        {
            if (!string.IsNullOrEmpty(variable?.Name) && properties.TryGetPropertyValue(variable.Name, out var value))
            {
                values[variable.Name] = value?.DeepClone();
            }
        }

        return values;
    }

    // How the Run dialog runs the published version: with its inputs when it starts with Started by Workflow,
    // otherwise with a request when it starts with an HTTP Request event.
    private static WorkflowDesignerRun RunOf(WorkflowType workflowType)
    {
        var starts = workflowType.Activities?.Where(activity => activity.IsStart).ToList() ?? [];
        var startedByWorkflow = starts.FirstOrDefault(activity => activity.Name == nameof(StartedByWorkflowEvent));

        if (startedByWorkflow is not null)
        {
            return new WorkflowDesignerRun
            {
                Mode = WorkflowDesignerRun.InputsMode,
                IsEnabled = workflowType.IsEnabled,
                ActivityId = startedByWorkflow.ActivityId,
                Inputs = (workflowType.Variables ?? []).Where(variable => variable.IsInput).ToList(),
            };
        }

        var httpRequest = starts.FirstOrDefault(activity => activity.Name == HttpRequestEvent.EventName);

        if (httpRequest is null)
        {
            return new WorkflowDesignerRun { IsEnabled = workflowType.IsEnabled };
        }

        // What the activities read from the request, which the dialog asks for.
        var (queryParameters, formFields) = WorkflowRequestNames.Find(workflowType.Activities);

        return new WorkflowDesignerRun
        {
            Mode = WorkflowDesignerRun.HttpMode,
            IsEnabled = workflowType.IsEnabled,
            ActivityId = httpRequest.ActivityId,
            HttpMethod = httpRequest.Properties?[nameof(HttpRequestEvent.HttpMethod)]?.GetValue<string>() ?? "GET",
            QueryParameters = queryParameters,
            FormFields = formFields,
        };
    }

    private string InstanceUrl(long instanceId)
        => Url.Action("Details", "Workflow", new { area = "OrchardCore.Workflows", id = instanceId });

    private static JsonNode SerializeValue(object value)
    {
        try
        {
            return value is JsonNode node ? node.DeepClone() : JsonSerializer.SerializeToNode(value, JOptions.Default);
        }
        catch (Exception ex) when (ex is NotSupportedException or InvalidOperationException or JsonException)
        {
            return JsonValue.Create(value?.ToString());
        }
    }

    private Task<bool> CanManageAsync()
        => _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows);

    // An instance of the workflow type, or a null workflow when the type doesn't have one with this id.
    private async Task<(WorkflowType WorkflowType, Workflow Workflow)> GetInstanceAsync(long workflowTypeId, long instanceId)
    {
        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        var workflow = workflowType is null ? null : await _workflowStore.GetAsync(instanceId);

        return workflow is null || workflow.WorkflowTypeId != workflowType.WorkflowTypeId
            ? (workflowType, null)
            : (workflowType, workflow);
    }

    private async Task<(WorkflowType WorkflowType, ActivityContext ActivityContext)> GetActivityAsync(long workflowTypeId, string activityId)
    {
        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null || string.IsNullOrEmpty(activityId))
        {
            return (workflowType, null);
        }

        var draft = await _draftManager.GetAsync(workflowType.WorkflowTypeId);
        var activities = draft?.Activities ?? workflowType.Activities;
        var record = activities.FirstOrDefault(activity => activity.ActivityId == activityId);

        if (record is null)
        {
            return (workflowType, null);
        }

        // Edit a copy, so a rejected change never touches the loaded draft or live type.
        var copy = record.Clone();
        var activityContext = await _workflowManager.CreateActivityExecutionContextAsync(copy, copy.Properties);

        return (workflowType, activityContext);
    }

    private ViewResult Fragment(WorkflowDesignerFragmentViewModel model)
    {
        var result = View(FragmentViewName, model);
        result.ContentType = "application/json; charset=utf-8";

        return result;
    }

    private ObjectResult FailureResult(WorkflowTypeDraftResult result)
    {
        switch (result.Status)
        {
            case WorkflowTypeDraftStatus.Conflict:
                return Problem(
                    title: S["The workflow was changed in the meantime."],
                    detail: S["Reload the workflow to get the latest changes, or overwrite them with yours."],
                    statusCode: StatusCodes.Status409Conflict,
                    extensions: new Dictionary<string, object>
                    {
                        ["currentRevision"] = result.Revision,
                        ["modifiedBy"] = result.ModifiedByUserName,
                        ["modifiedUtc"] = result.ModifiedUtc,
                    });

            case WorkflowTypeDraftStatus.Invalid when result.Issues.Count > 0:
                return Problem(
                    title: S["The workflow can't be published."],
                    detail: S["Fix the errors listed in the issues first."],
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object> { ["issues"] = result.Issues });

            case WorkflowTypeDraftStatus.NotFound:
                return this.ApiNotFoundProblem();

            default:
                return this.ApiBadRequestProblem();
        }
    }
}
