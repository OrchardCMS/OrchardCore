using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
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

namespace OrchardCore.Workflows.Controllers
{
    [Admin]
    public class WorkflowController : Controller
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
        protected readonly dynamic New;
        protected readonly IHtmlLocalizer H;
        protected readonly IStringLocalizer S;

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
            New = shapeFactory;
            H = htmlLocalizer;
            S = stringLocalizer;
        }

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

            var pagerShape = (await New.Pager(pager)).TotalItemCount(await query.CountAsync()).RouteData(routeData);
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

            model.Options.WorkflowsSorts = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["Recently created"], Value = nameof(WorkflowOrder.CreatedDesc) },
                new SelectListItem() { Text = S["Least recently created"], Value = nameof(WorkflowOrder.Created) },
            };

            model.Options.WorkflowsStatuses = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["All"], Value = nameof(WorkflowFilter.All) },
                new SelectListItem() { Text = S["Faulted"], Value = nameof(WorkflowFilter.Faulted) },
                new SelectListItem() { Text = S["Finished"], Value = nameof(WorkflowFilter.Finished) },
            };

            viewModel.Options.WorkflowsBulkAction = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["Delete"], Value = nameof(WorkflowBulkAction.Delete) },
            };

            return View(viewModel);
        }

        [HttpPost, ActionName(nameof(Index))]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(WorkflowIndexViewModel model)
        {
            return RedirectToAction(nameof(Index), new RouteValueDictionary
            {
                { "Options.Filter", model.Options.Filter },
                { "Options.OrderBy", model.Options.OrderBy },
            });
        }

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

            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var viewModel = new WorkflowViewModel
            {
                Workflow = workflow,
                WorkflowType = workflowType,
                WorkflowTypeJson = JsonConvert.SerializeObject(workflowTypeData, Formatting.None, jsonSerializerSettings),
                WorkflowJson = JsonConvert.SerializeObject(workflow, Formatting.Indented, jsonSerializerSettings),
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
                        throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Invalid bulk action.");
                }
            }
            return RedirectToAction(nameof(Index), new { workflowTypeId, pagenum = pagerParameters.Page, pagesize = pagerParameters.PageSize });
        }

        private async Task<dynamic> BuildActivityDisplayAsync(ActivityContext activityContext, long workflowTypeId, bool isBlocking, string displayType)
        {
            dynamic activityShape = await _activityDisplayManager.BuildDisplayAsync(activityContext.Activity, _updateModelAccessor.ModelUpdater, displayType);
            activityShape.Metadata.Type = $"Activity_{displayType}ReadOnly";
            activityShape.Activity = activityContext.Activity;
            activityShape.ActivityRecord = activityContext.ActivityRecord;
            activityShape.WorkflowTypeId = workflowTypeId;
            activityShape.IsBlocking = isBlocking;
            return activityShape;
        }
    }
}
