using System.Globalization;
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
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Localization;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
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
    // The statuses of the instances that are starting, running or resuming.
    private static readonly WorkflowStatus[] s_runningStatuses =
    [
        WorkflowStatus.Idle,
        WorkflowStatus.Starting,
        WorkflowStatus.Resuming,
        WorkflowStatus.Executing,
    ];

    private readonly PagerOptions _pagerOptions;
    private readonly ISession _session;
    private readonly IWorkflowManager _workflowManager;
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowTypeVersionStore _workflowTypeVersionStore;
    private readonly IWorkflowStore _workflowStore;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;
    private readonly IDistributedLock _distributedLock;
    private readonly IEnumerable<IJSLocalizer> _jsLocalizers;
    private readonly IWorkflowExecutionJournal _journal;
    private readonly IClock _clock;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public WorkflowController(
        IOptions<PagerOptions> pagerOptions,
        ISession session,
        IWorkflowManager workflowManager,
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowTypeVersionStore workflowTypeVersionStore,
        IWorkflowStore workflowStore,
        IAuthorizationService authorizationService,
        IShapeFactory shapeFactory,
        INotifier notifier,
        IHtmlLocalizer<WorkflowController> htmlLocalizer,
        IDistributedLock distributedLock,
        IStringLocalizer<WorkflowController> stringLocalizer,
        IEnumerable<IJSLocalizer> jsLocalizers,
        IWorkflowExecutionJournal journal,
        IClock clock)
    {
        _pagerOptions = pagerOptions.Value;
        _session = session;
        _workflowManager = workflowManager;
        _workflowTypeStore = workflowTypeStore;
        _workflowTypeVersionStore = workflowTypeVersionStore;
        _workflowStore = workflowStore;
        _authorizationService = authorizationService;
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        H = htmlLocalizer;
        _distributedLock = distributedLock;
        S = stringLocalizer;
        _jsLocalizers = jsLocalizers;
        _journal = journal;
        _clock = clock;
    }

    [Admin("Workflows/Types/{workflowTypeId}/Instances/{action}", "Workflows")]
    public async Task<IActionResult> Index(long workflowTypeId, WorkflowIndexViewModel model, PagerParameters pagerParameters, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

        if (workflowType is null)
        {
            return NotFound();
        }

        return View(await BuildListAsync(workflowType, model, pagerParameters, returnUrl));
    }

    /// <summary>
    /// The instances of every workflow type.
    /// </summary>
    [Admin("Workflows/Instances", "WorkflowInstances")]
    public async Task<IActionResult> All(WorkflowIndexViewModel model, PagerParameters pagerParameters, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        return View(nameof(Index), await BuildListAsync(null, model, pagerParameters, returnUrl));
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(WorkflowIndexViewModel model)
        => RedirectToAction(nameof(Index), FilterRouteValues(model.Options));

    [HttpPost, ActionName(nameof(All))]
    [FormValueRequired("submit.Filter")]
    public ActionResult AllFilterPOST(WorkflowIndexViewModel model)
        => RedirectToAction(nameof(All), FilterRouteValues(model.Options));

    private static RouteValueDictionary FilterRouteValues(WorkflowIndexOptions options)
    {
        var values = new RouteValueDictionary
        {
            { "Options.Filter", options.Filter },
            { "Options.OrderBy", options.OrderBy },
            { "Options.Created", options.Created },
        };

        if (!string.IsNullOrEmpty(options.WorkflowTypeId))
        {
            values.Add("Options.WorkflowTypeId", options.WorkflowTypeId);
        }

        return values;
    }

    // The list of the instances of a workflow type, or of every workflow type when it's null.
    private async Task<WorkflowIndexViewModel> BuildListAsync(WorkflowType workflowType, WorkflowIndexViewModel model, PagerParameters pagerParameters, string returnUrl)
    {
        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = Url.Action(nameof(WorkflowTypeController.Index), typeof(WorkflowTypeController).ControllerName());
        }

        var options = model.Options;
        var workflowTypeId = workflowType?.WorkflowTypeId ?? (string.IsNullOrEmpty(options.WorkflowTypeId) ? null : options.WorkflowTypeId);
        DateTime? createdAfter = options.Created switch
        {
            WorkflowCreatedFilter.Last24Hours => _clock.UtcNow.AddDays(-1),
            WorkflowCreatedFilter.Last7Days => _clock.UtcNow.AddDays(-7),
            WorkflowCreatedFilter.Last30Days => _clock.UtcNow.AddDays(-30),
            _ => null,
        };

        // The instances of the workflow type and the date, with a status filter.
        IQueryIndex<WorkflowIndex> Query(WorkflowFilter filter)
        {
            var query = _session.QueryIndex<WorkflowIndex>();

            if (workflowTypeId is not null)
            {
                query = query.Where(x => x.WorkflowTypeId == workflowTypeId);
            }

            if (createdAfter is { } after)
            {
                query = query.Where(x => x.CreatedUtc >= after);
            }

            return filter switch
            {
                WorkflowFilter.Finished => query.Where(x => x.WorkflowStatus == WorkflowStatus.Finished),
                WorkflowFilter.Faulted => query.Where(x => x.WorkflowStatus == WorkflowStatus.Faulted),
                WorkflowFilter.Halted => query.Where(x => x.WorkflowStatus == WorkflowStatus.Halted),
                WorkflowFilter.Running => query.Where(x => x.WorkflowStatus.IsIn(s_runningStatuses)),
                WorkflowFilter.Aborted => query.Where(x => x.WorkflowStatus == WorkflowStatus.Aborted),
                _ => query,
            };
        }

        var statusCounts = new Dictionary<WorkflowFilter, int>();

        foreach (var filter in Enum.GetValues<WorkflowFilter>())
        {
            statusCounts[filter] = await Query(filter).CountAsync();
        }

        var query = options.OrderBy switch
        {
            WorkflowOrder.Created => Query(options.Filter).OrderBy(x => x.CreatedUtc),
            _ => Query(options.Filter).OrderByDescending(x => x.CreatedUtc),
        };

        var pager = new Pager(pagerParameters, _pagerOptions);

        var routeData = new RouteData();
        routeData.Values.Add("Options.Filter", options.Filter);
        routeData.Values.Add("Options.OrderBy", options.OrderBy);
        routeData.Values.Add("Options.Created", options.Created);

        if (workflowType is null && workflowTypeId is not null)
        {
            routeData.Values.Add("Options.WorkflowTypeId", workflowTypeId);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, statusCounts[options.Filter], routeData);

        var pageOfItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();

        var workflowIds = pageOfItems.Select(item => item.WorkflowId);
        var workflowsQuery = _session.Query<Workflow, WorkflowIndex>(item => item.WorkflowId.IsIn(workflowIds));

        workflowsQuery = options.OrderBy switch
        {
            WorkflowOrder.Created => workflowsQuery.OrderBy(i => i.CreatedUtc),
            _ => workflowsQuery.OrderByDescending(i => i.CreatedUtc),
        };

        var workflows = (await workflowsQuery.ListAsync()).ToList();

        // The workflow types of the instances, and the number of the version each instance runs on.
        var workflowTypes = workflowType is not null
            ? new Dictionary<string, WorkflowType> { [workflowType.WorkflowTypeId] = workflowType }
            : (await _workflowTypeStore.ListAsync()).ToDictionary(type => type.WorkflowTypeId);

        var versionNumbers = new Dictionary<string, int>();

        foreach (var typeId in workflows.Select(workflow => workflow.WorkflowTypeId).Distinct())
        {
            foreach (var version in await _workflowTypeVersionStore.ListAsync(typeId))
            {
                versionNumbers[version.VersionId] = version.Version;
            }
        }

        var viewModel = new WorkflowIndexViewModel
        {
            WorkflowType = workflowType,
            Workflows = workflows
                .Select(x =>
                {
                    workflowTypes.TryGetValue(x.WorkflowTypeId, out var type);

                    return new WorkflowEntry
                    {
                        Workflow = x,
                        Id = x.Id,
                        WorkflowType = type,
                        Version = x.WorkflowTypeVersionId is not null && versionNumbers.TryGetValue(x.WorkflowTypeVersionId, out var number) ? number : null,
                        IsPublishedVersion = x.WorkflowTypeVersionId is not null && x.WorkflowTypeVersionId == type?.VersionId,
                    };
                })
                .ToList(),
            Options = options,
            Pager = pagerShape,
            ReturnUrl = returnUrl,
            StatusCounts = statusCounts,
        };

        options.WorkflowsSorts =
        [
            new SelectListItem(S["Recently created"], nameof(WorkflowOrder.CreatedDesc)),
            new SelectListItem(S["Least recently created"], nameof(WorkflowOrder.Created)),
        ];

        options.WorkflowsStatuses =
        [
            new SelectListItem(S["All"], nameof(WorkflowFilter.All)),
            new SelectListItem(S["Halted"], nameof(WorkflowFilter.Halted)),
            new SelectListItem(S["Running"], nameof(WorkflowFilter.Running)),
            new SelectListItem(S["Faulted"], nameof(WorkflowFilter.Faulted)),
            new SelectListItem(S["Finished"], nameof(WorkflowFilter.Finished)),
            new SelectListItem(S["Aborted"], nameof(WorkflowFilter.Aborted)),
        ];

        options.WorkflowsCreated =
        [
            new SelectListItem(S["Any time"], nameof(WorkflowCreatedFilter.Any)),
            new SelectListItem(S["Last 24 hours"], nameof(WorkflowCreatedFilter.Last24Hours)),
            new SelectListItem(S["Last 7 days"], nameof(WorkflowCreatedFilter.Last7Days)),
            new SelectListItem(S["Last 30 days"], nameof(WorkflowCreatedFilter.Last30Days)),
        ];

        if (workflowType is null)
        {
            options.WorkflowTypes =
            [
                new SelectListItem(S["All workflows"], string.Empty),
                .. workflowTypes.Values
                    .OrderBy(type => type.Name, StringComparer.CurrentCultureIgnoreCase)
                    .Select(type => new SelectListItem(type.Name, type.WorkflowTypeId)),
            ];
        }

        options.WorkflowsBulkAction =
        [
            new SelectListItem(S["Retry"], nameof(WorkflowBulkAction.Retry)),
            new SelectListItem(S["Cancel"], nameof(WorkflowBulkAction.Cancel)),
            new SelectListItem(S["Delete"], nameof(WorkflowBulkAction.Delete)),
        ];

        return viewModel;
    }

    public async Task<IActionResult> Details(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflow = await _workflowStore.GetAsync(id);

        if (workflow == null)
        {
            return NotFound();
        }

        var workflowType = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);

        var viewModel = new WorkflowViewModel
        {
            Workflow = workflow,
            WorkflowType = workflowType,
            WorkflowJson = JConvert.SerializeObject(workflow, JOptions.CamelCaseIndented),
            DesignerConfigJson = WorkflowDesignerConfigBuilder.Build(
                Url,
                User,
                _jsLocalizers,
                workflowType,
                workflow,
                canRetry: await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ExecuteWorkflows)),
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
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
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
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

        // If a singleton, try to acquire a lock per workflow type, or per correlation id.
        (var locker, var locked) = await _distributedLock.TryAcquireWorkflowTypeLockAsync(workflowType, correlationId: workflow.CorrelationId);
        if (!locked)
        {
            await _notifier.ErrorAsync(H["Another instance is already running.", id]);
        }
        else
        {
            await using var acquiredLock = locker;

            // Check if this is a workflow singleton and there's already an halted instance on any activity, or one with
            // the same correlation id when it runs one instance per correlation id.
            if ((workflowType.IsSingleton && await _workflowStore.HasHaltedInstanceAsync(workflowType.WorkflowTypeId)) ||
                (workflowType.IsSingletonPerCorrelation &&
                    !string.IsNullOrEmpty(workflow.CorrelationId) &&
                    await _workflowStore.HasHaltedInstanceAsync(workflowType.WorkflowTypeId, workflow.CorrelationId)))
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
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        var result = await ApplyBulkActionAsync(options.BulkAction, itemIds);

        return result ?? RedirectToAction(nameof(Index), new { workflowTypeId, pagenum = pagerParameters.Page, pagesize = pagerParameters.PageSize });
    }

    [HttpPost]
    [ActionName(nameof(All))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<IActionResult> AllBulkEdit(WorkflowIndexOptions options, PagerParameters pagerParameters, IEnumerable<long> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        var result = await ApplyBulkActionAsync(options.BulkAction, itemIds);

        if (result is not null)
        {
            return result;
        }

        var routeValues = FilterRouteValues(options);
        routeValues.Add("pagenum", pagerParameters.Page);
        routeValues.Add("pagesize", pagerParameters.PageSize);

        return RedirectToAction(nameof(All), routeValues);
    }

    // Applies a bulk action to the checked instances; returns a result when the action isn't allowed.
    private async Task<IActionResult> ApplyBulkActionAsync(WorkflowBulkAction action, IEnumerable<long> itemIds)
    {
        if (itemIds?.Any() != true || action == WorkflowBulkAction.None)
        {
            return null;
        }

        if (action is not (WorkflowBulkAction.Delete or WorkflowBulkAction.Retry or WorkflowBulkAction.Cancel))
        {
            return BadRequest();
        }

        if (action == WorkflowBulkAction.Retry && !await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ExecuteWorkflows))
        {
            return Forbid();
        }

        var workflows = (await _workflowStore.GetAsync(itemIds.Distinct())).Where(workflow => workflow is not null).ToList();

        switch (action)
        {
            case WorkflowBulkAction.Delete:
                var deletedWorkflowIds = new List<string>();

                foreach (var workflow in workflows)
                {
                    await _workflowStore.DeleteAsync(workflow);
                    deletedWorkflowIds.Add(workflow.Id.ToString(CultureInfo.InvariantCulture));
                }

                if (deletedWorkflowIds.Count > 0)
                {
                    await _notifier.SuccessAsync(H.Plural(deletedWorkflowIds.Count, "The workflow \"{1}\" has been deleted.", "The following workflows have been deleted: {1}.", string.Join(", ", deletedWorkflowIds)));
                }

                break;

            case WorkflowBulkAction.Retry:
                var retried = 0;
                var notRetried = 0;

                foreach (var workflow in workflows.Where(workflow => workflow.Status == WorkflowStatus.Faulted))
                {
                    // The activity the instance faulted at: the one its retry policy retries, or the journal's.
                    var activityId = workflow.PendingRetry?.ActivityId
                        ?? (await _journal.ListAsync(workflow.WorkflowId)).LastOrDefault(record => record.Status == WorkflowExecutionRecordStatus.Faulted)?.ActivityId;

                    try
                    {
                        if (activityId is not null && await _workflowManager.RetryActivityAsync(workflow, activityId) is not null)
                        {
                            retried++;

                            continue;
                        }
                    }
                    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                    {
                        // The activity isn't in the instance's definition any more, or its workflow type is gone.
                    }

                    notRetried++;
                }

                if (retried > 0)
                {
                    await _notifier.SuccessAsync(H.Plural(retried, "1 faulted instance ran again.", "{0} faulted instances ran again."));
                }

                if (notRetried > 0)
                {
                    await _notifier.WarningAsync(H.Plural(notRetried, "1 faulted instance couldn't be retried: the activity it faulted at isn't known, or it's running.", "{0} faulted instances couldn't be retried: the activity they faulted at isn't known, or they're running."));
                }

                break;

            case WorkflowBulkAction.Cancel:
                var canceled = 0;

                foreach (var workflow in workflows.Where(workflow => workflow.Status is not (WorkflowStatus.Finished or WorkflowStatus.Aborted)))
                {
                    workflow.Status = WorkflowStatus.Aborted;
                    workflow.BlockingActivities.Clear();
                    workflow.PendingRetry = null;
                    await _workflowStore.SaveAsync(workflow);
                    canceled++;
                }

                if (canceled > 0)
                {
                    await _notifier.SuccessAsync(H.Plural(canceled, "1 instance was canceled.", "{0} instances were canceled."));
                }

                break;
        }

        return null;
    }
}
