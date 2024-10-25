using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Locking.Distributed;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Workflows.Controllers;

[Admin]
public sealed class WorkflowController : Controller
{
    private readonly PagerOptions _pagerOptions;
    private readonly ISession _session;
    private readonly IWorkflowManager _workflowManager;
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowStore _workflowStore;
    private readonly IAuthorizationService _authorizationService;
    private readonly IActivityDisplayManager _activityDisplayManager;
    private readonly INotifier _notifier;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShapeFactory _shapeFactory;
    private readonly IDistributedLock _distributedLock;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public WorkflowController(
        IOptions<PagerOptions> pagerOptions,
        ISession session,
        IWorkflowManager workflowManager,
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowStore workflowStore,
        IAuthorizationService authorizationService,
        IActivityDisplayManager activityDisplayManager,
        IShapeFactory shapeFactory,
        INotifier notifier,
        IHtmlLocalizer<WorkflowController> htmlLocalizer,
        IDistributedLock distributedLock,
        IStringLocalizer<WorkflowController> stringLocalizer,
        IUpdateModelAccessor updateModelAccessor)
    {
        _pagerOptions = pagerOptions.Value;
        _session = session;
        _workflowManager = workflowManager;
        _workflowTypeStore = workflowTypeStore;
        _workflowStore = workflowStore;
        _authorizationService = authorizationService;
        _activityDisplayManager = activityDisplayManager;
        _notifier = notifier;
        _updateModelAccessor = updateModelAccessor;
        _shapeFactory = shapeFactory;
        H = htmlLocalizer;
        _distributedLock = distributedLock;
        S = stringLocalizer;
    }

    [Admin("Workflows/Types/{workflowTypeId}/Instances/{action}", "Workflows")]
    public async Task<IActionResult> Index(long workflowTypeId, WorkflowIndexViewModel model, PagerParameters pagerParameters, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = Url.Action(nameof(WorkflowTypeController.Index), typeof(WorkflowTypeController).ControllerName());
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        var query = _session.QueryIndex<WorkflowIndex>()
            .Where(x => x.WorkflowTypeId == workflowType.WorkflowTypeId);

        query = model.Options.Filter switch
        {
            WorkflowFilter.Finished => query.Where(x => x.WorkflowStatus == (int)WorkflowStatus.Finished),
            WorkflowFilter.Faulted => query.Where(x => x.WorkflowStatus == (int)WorkflowStatus.Faulted),
            _ => query,
        };

        query = model.Options.OrderBy switch
        {
            WorkflowOrder.Created => query.OrderBy(x => x.CreatedUtc),
            _ => query.OrderByDescending(x => x.CreatedUtc),
        };

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var routeData = new RouteData();
        routeData.Values.Add("Filter", model.Options.Filter);

        var pagerShape = await _shapeFactory.PagerAsync(pager, await query.CountAsync(), routeData);

        var pageOfItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();

        var workflowIds = pageOfItems.Select(item => item.WorkflowId);
        var workflowsQuery = _session.Query<Workflow, WorkflowIndex>(item => item.WorkflowId.IsIn(workflowIds));

        workflowsQuery = model.Options.OrderBy switch
        {
            WorkflowOrder.Created => workflowsQuery.OrderBy(i => i.CreatedUtc),
            _ => workflowsQuery.OrderByDescending(i => i.CreatedUtc),
        };

        var workflows = await workflowsQuery.ListAsync();

        var viewModel = new WorkflowIndexViewModel
        {
            WorkflowType = workflowType,
            Workflows = workflows.Select(x => new WorkflowEntry { Workflow = x, Id = x.Id }).ToList(),
            Options = model.Options,
            Pager = pagerShape,
            ReturnUrl = returnUrl,
        };

        model.Options.WorkflowsSorts =
        [
            new SelectListItem(S["Recently created"], nameof(WorkflowOrder.CreatedDesc)),
            new SelectListItem(S["Least recently created"], nameof(WorkflowOrder.Created)),
        ];

        model.Options.WorkflowsStatuses =
        [
            new SelectListItem(S["All"], nameof(WorkflowFilter.All)),
            new SelectListItem(S["Faulted"], nameof(WorkflowFilter.Faulted)),
            new SelectListItem(S["Finished"], nameof(WorkflowFilter.Finished)),
        ];

        viewModel.Options.WorkflowsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(WorkflowBulkAction.Delete)),
        ];

        return View(viewModel);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(WorkflowIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { "Options.Filter", model.Options.Filter },
            { "Options.OrderBy", model.Options.OrderBy },
        });

    public async Task<IActionResult> Details(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflow = await _workflowStore.GetAsync(id);

        if (workflow == null)
        {
            return NotFound();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);
        var blockingActivities = workflow.BlockingActivities.ToDictionary(x => x.ActivityId);
        var workflowContext = await _workflowManager.CreateWorkflowExecutionContextAsync(workflowType, workflow);
        var activityContexts = await Task.WhenAll(workflowType.Activities.Select(x => _workflowManager.CreateActivityExecutionContextAsync(x, x.Properties)));

        var activityDesignShapes = new List<dynamic>();

        foreach (var activityContext in activityContexts)
        {
            activityDesignShapes.Add(await BuildActivityDisplayAsync(activityContext, workflowType.Id, blockingActivities.ContainsKey(activityContext.ActivityRecord.ActivityId), "Design"));
        }

        var activitiesDataQuery = activityContexts.Select(x => new
        {
            Id = x.ActivityRecord.ActivityId,
            x.ActivityRecord.X,
            x.ActivityRecord.Y,
            x.ActivityRecord.Name,
            x.ActivityRecord.IsStart,
            IsEvent = x.Activity.IsEvent(),
            IsBlocking = workflow.BlockingActivities.Any(a => a.ActivityId == x.ActivityRecord.ActivityId),
            Outcomes = x.Activity.GetPossibleOutcomes(workflowContext, x).ToArray(),
        });
        var workflowTypeData = new
        {
            workflowType.Id,
            workflowType.Name,
            workflowType.IsEnabled,
            Activities = activitiesDataQuery.ToArray(),
            workflowType.Transitions,
        };

        var viewModel = new WorkflowViewModel
        {
            Workflow = workflow,
            WorkflowType = workflowType,
            WorkflowTypeJson = JConvert.SerializeObject(workflowTypeData, JOptions.CamelCase),
            WorkflowJson = JConvert.SerializeObject(workflow, JOptions.CamelCaseIndented),
            ActivityDesignShapes = activityDesignShapes,
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflow = await _workflowStore.GetAsync(id);

        if (workflow == null)
        {
            return NotFound();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);
        await _workflowStore.DeleteAsync(workflow);
        await _notifier.SuccessAsync(H["Workflow {0} has been deleted.", id]);
        return RedirectToAction(nameof(Index), new { workflowTypeId = workflowType.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Restart(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflow = await _workflowStore.GetAsync(id);

        if (workflow == null)
        {
            return NotFound();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);

        if (workflowType == null)
        {
            return NotFound();
        }

        // If a singleton, try to acquire a lock per workflow type.
        (var locker, var locked) = await _distributedLock.TryAcquireWorkflowTypeLockAsync(workflowType);
        if (!locked)
        {
            await _notifier.ErrorAsync(H["Another instance is already running.", id]);
        }
        else
        {
            await using var acquiredLock = locker;

            // Check if this is a workflow singleton and there's already an halted instance on any activity.
            if (workflowType.IsSingleton && await _workflowStore.HasHaltedInstanceAsync(workflowType.WorkflowTypeId))
            {
                await _notifier.ErrorAsync(H["Another instance is already running.", id]);
            }
            else
            {
                await _workflowManager.RestartWorkflowAsync(workflow, workflowType);

                await _notifier.SuccessAsync(H["Workflow {0} has been restarted.", id]);
            }
        }

        return RedirectToAction(nameof(Index), new { workflowTypeId = workflowType.Id });
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<IActionResult> BulkEdit(long workflowTypeId, WorkflowIndexOptions options, PagerParameters pagerParameters, IEnumerable<long> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var checkedEntries = await _session.Query<Workflow, WorkflowIndex>().Where(x => x.DocumentId.IsIn(itemIds)).ListAsync();
            switch (options.BulkAction)
            {
                case WorkflowBulkAction.None:
                    break;
                case WorkflowBulkAction.Delete:
                    foreach (var entry in checkedEntries)
                    {
                        var workflow = await _workflowStore.GetAsync(entry.Id);

                        if (workflow != null)
                        {
                            await _workflowStore.DeleteAsync(workflow);
                            await _notifier.SuccessAsync(H["Workflow {0} has been deleted.", workflow.Id]);
                        }
                    }
                    break;

                default:
                    return BadRequest();
            }
        }
        return RedirectToAction(nameof(Index), new { workflowTypeId, pagenum = pagerParameters.Page, pagesize = pagerParameters.PageSize });
    }

    private async Task<dynamic> BuildActivityDisplayAsync(ActivityContext activityContext, long workflowTypeId, bool isBlocking, string displayType)
    {
        var activityShape = await _activityDisplayManager.BuildDisplayAsync(activityContext.Activity, _updateModelAccessor.ModelUpdater, displayType);
        activityShape.Metadata.Type = $"Activity_{displayType}ReadOnly";
        activityShape.Properties["Activity"] = activityContext.Activity;
        activityShape.Properties["ActivityRecord"] = activityContext.ActivityRecord;
        activityShape.Properties["WorkflowTypeId"] = workflowTypeId;
        activityShape.Properties["IsBlocking"] = isBlocking;

        return activityShape;
    }
}
