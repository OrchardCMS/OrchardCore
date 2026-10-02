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
    private readonly IWorkflowTypeDraftManager _draftManager;
    private readonly WorkflowDesignerModelBuilder _modelBuilder;
    private readonly IWorkflowManager _workflowManager;
    private readonly IActivityDisplayManager _activityDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly ISession _session;
    private readonly IClock _clock;

    internal readonly IStringLocalizer S;

    public WorkflowDesignerController(
        IAuthorizationService authorizationService,
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowTypeDraftManager draftManager,
        WorkflowDesignerModelBuilder modelBuilder,
        IWorkflowManager workflowManager,
        IActivityDisplayManager activityDisplayManager,
        IUpdateModelAccessor updateModelAccessor,
        ISession session,
        IClock clock,
        IStringLocalizer<WorkflowDesignerController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _workflowTypeStore = workflowTypeStore;
        _draftManager = draftManager;
        _modelBuilder = modelBuilder;
        _workflowManager = workflowManager;
        _activityDisplayManager = activityDisplayManager;
        _updateModelAccessor = updateModelAccessor;
        _session = session;
        _clock = clock;
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
            Settings = new WorkflowTypeDraftSettings
            {
                Name = source.Name,
                IsEnabled = source.IsEnabled,
                IsSingleton = source.IsSingleton,
                LockTimeout = source.LockTimeout,
                LockExpiration = source.LockExpiration,
                DeleteFinishedWorkflows = source.DeleteFinishedWorkflows,
            },
            Nodes = await _modelBuilder.BuildNodesAsync(source),
            Transitions = source.Transitions.Select(WorkflowDesignerTransition.From).ToList(),
            Issues = issues,
            RunningInstanceCount = runningInstanceCount,
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
