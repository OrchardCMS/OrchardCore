using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
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

    private static readonly WorkflowStatus[] s_runningStatuses =
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
        S = stringLocalizer;
    }

    /// <summary>
    /// The designer's first URL; the designer is now at <c>WorkflowType/Edit</c>.
    /// </summary>
    [HttpGet]
    public IActionResult Index(long workflowTypeId, string activityId = null)
        => RedirectToAction("Edit", "WorkflowType", new { area = "OrchardCore.Workflows", id = workflowTypeId, activityId });

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
        var issues = draft is null
            ? await _draftManager.ValidateAsync(workflowType)
            : await _draftManager.ValidateAsync(draft);

        var runningInstanceCount = await _session.QueryIndex<WorkflowIndex>(index =>
                index.WorkflowTypeId == workflowType.WorkflowTypeId &&
                index.WorkflowStatus.IsIn(s_runningStatuses))
            .CountAsync();

        return Ok(new WorkflowDesignerDefinition
        {
            Id = workflowType.Id,
            WorkflowTypeId = workflowType.WorkflowTypeId,
            Revision = draft?.Revision ?? 0,
            HasDraft = draft is not null,
            DraftModifiedBy = draft?.ModifiedByUserName,
            DraftModifiedByUserId = draft?.ModifiedByUserId,
            DraftModifiedUtc = draft?.ModifiedUtc,
            Settings = SettingsOf(source),
            Nodes = await _modelBuilder.BuildNodesAsync(source),
            Transitions = source.Transitions.Select(WorkflowDesignerTransition.From).ToList(),
            Variables = [.. source.Variables ?? []],
            VariableTypes = VariableTypes(),
            Issues = issues,
            RunningInstanceCount = runningInstanceCount,
            PublishedVersion = await PublishedVersionAsync(workflowType),
        });
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

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        var workflow = workflowType is null ? null : await _workflowStore.GetAsync(instanceId);

        if (workflow is null || workflow.WorkflowTypeId != workflowType.WorkflowTypeId)
        {
            return this.ApiNotFoundProblem();
        }

        var definition = await _versionStore.GetWorkflowTypeAsync(workflowType, workflow.WorkflowTypeVersionId);
        var version = string.IsNullOrEmpty(workflow.WorkflowTypeVersionId) ? null : await _versionStore.GetAsync(workflow.WorkflowTypeVersionId);
        var journal = await _journal.ListAsync(workflow.WorkflowId, JournalRecordCount);

        return Ok(new WorkflowDesignerDefinition
        {
            Id = workflowType.Id,
            WorkflowTypeId = workflowType.WorkflowTypeId,
            Settings = SettingsOf(definition),
            Nodes = await _modelBuilder.BuildNodesAsync(definition),
            Transitions = definition.Transitions.Select(WorkflowDesignerTransition.From).ToList(),
            Variables = [.. definition.Variables ?? []],
            VariableTypes = VariableTypes(),
            PublishedVersion = await PublishedVersionAsync(workflowType),
            Version = WorkflowDesignerVersion.From(version, workflowType),
            Instance = new WorkflowDesignerInstance
            {
                Id = workflow.Id,
                WorkflowId = workflow.WorkflowId,
                Status = workflow.Status.ToString(),
                BlockingActivityIds = workflow.BlockingActivities.Select(activity => activity.ActivityId).Distinct().ToList(),
                VariableValues = VariableValuesOf(workflow, definition.Variables),
                FaultMessage = workflow.FaultMessage,
                FaultedActivityId = workflow.Status == WorkflowStatus.Faulted
                    ? journal.LastOrDefault(record => record.Status == WorkflowExecutionRecordStatus.Faulted)?.ActivityId
                    : null,
                Journal = journal.Select(WorkflowDesignerJournalRecord.From).ToList(),
                ExecutedActivityCounts = journal
                    .GroupBy(record => record.ActivityId)
                    .ToDictionary(group => group.Key, group => group.Count()),
                ExecutedTransitionCounts = ExecutedTransitionCounts(journal, definition),
            },
        });
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

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        var workflow = workflowType is null ? null : await _workflowStore.GetAsync(request.InstanceId);

        if (workflow is null || workflow.WorkflowTypeId != workflowType.WorkflowTypeId)
        {
            return this.ApiNotFoundProblem();
        }

        if (workflow.Status != WorkflowStatus.Faulted)
        {
            return ProblemResult(StatusCodes.Status400BadRequest, S["Only a faulted instance can be retried."]);
        }

        WorkflowExecutionContext workflowContext;

        try
        {
            workflowContext = await _workflowManager.RetryActivityAsync(workflow, request.ActivityId);
        }
        catch (ArgumentException)
        {
            return ProblemResult(StatusCodes.Status400BadRequest, S["The instance's definition doesn't have this activity."]);
        }

        if (workflowContext is null)
        {
            return ProblemResult(StatusCodes.Status409Conflict, S["The instance is running. Try again in a moment."]);
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

        if (string.IsNullOrEmpty(request?.Name))
        {
            return this.ApiBadRequestProblem();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return this.ApiNotFoundProblem();
        }

        var result = await _draftManager.AddActivityAsync(workflowType.WorkflowTypeId, request.Revision, request.Name, request.X, request.Y);

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
            var problem = ProblemDetailsFactory.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest,
                S["The variables can't be saved."],
                detail: S["Fix the errors listed in the variable errors first."]);

            problem.Extensions["variableErrors"] = errors;

            return new ObjectResult(problem)
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentTypes = { "application/problem+json" },
            };
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

        return Fragment(new WorkflowDesignerFragmentViewModel
        {
            PartialName = SettingsPartialName,
            PartialModel = new WorkflowTypePropertiesViewModel
            {
                Id = workflowType.Id,
                Name = source.Name,
                IsEnabled = source.IsEnabled,
                IsSingleton = source.IsSingleton,
                LockTimeout = source.LockTimeout,
                LockExpiration = source.LockExpiration,
                DeleteFinishedWorkflows = source.DeleteFinishedWorkflows,
                IsActivity = source.IsActivity,
            },
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
            IsSingleton = model.IsSingleton,
            LockTimeout = model.LockTimeout,
            LockExpiration = model.LockExpiration,
            DeleteFinishedWorkflows = model.DeleteFinishedWorkflows,
            IsActivity = model.IsActivity,
        };

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

        var instanceCounts = (await _session.QueryIndex<WorkflowIndex>(index => index.WorkflowTypeId == workflowType.WorkflowTypeId).ListAsync())
            .Where(index => !string.IsNullOrEmpty(index.WorkflowTypeVersionId))
            .GroupBy(index => index.WorkflowTypeVersionId)
            .ToDictionary(group => group.Key, group => group.Count());

        var versions = await _versionStore.ListAsync(workflowType.WorkflowTypeId);
        var draft = await _draftManager.GetAsync(workflowType.WorkflowTypeId);

        return Ok(new
        {
            versions = versions
                .Select(version => WorkflowDesignerVersion.From(version, workflowType, instanceCounts.GetValueOrDefault(version.VersionId)))
                .ToList(),
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
        var version = workflowType is null || string.IsNullOrEmpty(versionId) ? null : await _versionStore.GetAsync(versionId);

        if (version is null || version.WorkflowTypeId != workflowType.WorkflowTypeId)
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
        var version = workflowType is null || string.IsNullOrEmpty(request?.VersionId) ? null : await _versionStore.GetAsync(request.VersionId);

        if (version is null || version.WorkflowTypeId != workflowType.WorkflowTypeId)
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
        => new()
        {
            Name = workflowType.Name,
            IsEnabled = workflowType.IsEnabled,
            IsSingleton = workflowType.IsSingleton,
            LockTimeout = workflowType.LockTimeout,
            LockExpiration = workflowType.LockExpiration,
            DeleteFinishedWorkflows = workflowType.DeleteFinishedWorkflows,
            IsActivity = workflowType.IsActivity,
        };

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

        var version = string.IsNullOrEmpty(id) ? null : await _versionStore.GetAsync(id);

        return version is null || version.WorkflowTypeId != workflowType.WorkflowTypeId
            ? (null, null)
            : (ToDefinition(version, workflowType), version);
    }

    // A definition that is only shown: a version, or the draft (when version is null).
    private async Task<WorkflowDesignerDefinition> ReadOnlyDefinitionAsync(WorkflowType workflowType, WorkflowType definition, WorkflowTypeVersion version)
        => new()
        {
            Id = workflowType.Id,
            WorkflowTypeId = workflowType.WorkflowTypeId,
            HasDraft = version is null,
            Settings = SettingsOf(definition),
            Nodes = await _modelBuilder.BuildNodesAsync(definition),
            Transitions = definition.Transitions.Select(WorkflowDesignerTransition.From).ToList(),
            Variables = [.. definition.Variables ?? []],
            VariableTypes = VariableTypes(),
            PublishedVersion = await PublishedVersionAsync(workflowType),
            Version = WorkflowDesignerVersion.From(version, workflowType),
        };

    // The transitions the journal's outcomes took, as the engine follows them: the first transition of each outcome.
    private static Dictionary<string, int> ExecutedTransitionCounts(IReadOnlyList<WorkflowExecutionRecord> journal, WorkflowType definition)
    {
        var counts = new Dictionary<string, int>();

        foreach (var record in journal)
        {
            foreach (var outcome in record.Outcomes ?? [])
            {
                var transition = definition.Transitions.FirstOrDefault(x => x.SourceActivityId == record.ActivityId && x.SourceOutcomeName == outcome);

                if (transition is not null)
                {
                    var key = WorkflowDesignIssue.GetTransitionKey(transition);
                    counts[key] = counts.GetValueOrDefault(key) + 1;
                }
            }
        }

        return counts;
    }

    private List<WorkflowDesignerVariableType> VariableTypes()
        => _variableTypes.List()
            .Select(type => new WorkflowDesignerVariableType
            {
                Name = type.Name,
                DisplayName = type.DisplayName?.Value ?? type.Name,
                Editor = type.Editor,
            })
            .ToList();

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

    private Task<bool> CanManageAsync()
        => _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows);

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

    private ObjectResult ProblemResult(int status, string title)
    {
        var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, status, title);

        return new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
    }

    private ObjectResult FailureResult(WorkflowTypeDraftResult result)
    {
        switch (result.Status)
        {
            case WorkflowTypeDraftStatus.Conflict:
                var conflict = ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status409Conflict,
                    S["The workflow was changed in the meantime."],
                    detail: S["Reload the workflow to get the latest changes, or overwrite them with yours."]);

                conflict.Extensions["currentRevision"] = result.Revision;
                conflict.Extensions["modifiedBy"] = result.ModifiedByUserName;
                conflict.Extensions["modifiedUtc"] = result.ModifiedUtc;

                return new ObjectResult(conflict)
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    ContentTypes = { "application/problem+json" },
                };

            case WorkflowTypeDraftStatus.Invalid when result.Issues.Count > 0:
                var invalid = ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status400BadRequest,
                    S["The workflow can't be published."],
                    detail: S["Fix the errors listed in the issues first."]);

                invalid.Extensions["issues"] = result.Issues;

                return new ObjectResult(invalid)
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentTypes = { "application/problem+json" },
                };

            case WorkflowTypeDraftStatus.NotFound:
                return this.ApiNotFoundProblem();

            default:
                return this.ApiBadRequestProblem();
        }
    }
}
