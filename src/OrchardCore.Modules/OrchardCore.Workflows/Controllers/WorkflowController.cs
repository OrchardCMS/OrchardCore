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
        private readonly IHtmlLocalizer H;
        private readonly IStringLocalizer S;

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

        private dynamic New { get; }

        public async Task<IActionResult> Index(int workflowTypeId, WorkflowIndexViewModel model, PagerParameters pagerParameters, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Forbid();
            }

            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Action(nameof(Index), "WorkflowType");
            }

            var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

            var query = _session.Query<Workflow, WorkflowIndex>();
            query = query.Where(x => x.WorkflowTypeId == workflowType.WorkflowTypeId);

            switch (model.Options.Filter)
            {
                case WorkflowFilter.Finished:
                    query = query.Where(x => x.WorkflowStatus == (int)WorkflowStatus.Finished);
                    break;
                case WorkflowFilter.Faulted:
                    query = query.Where(x => x.WorkflowStatus == (int)WorkflowStatus.Faulted);
                    break;
                case WorkflowFilter.All:
                default:
                    break;
            }

            switch (model.Options.OrderBy)
            {
                case WorkflowOrder.CreatedDesc:
                    query = query.OrderByDescending(x => x.CreatedUtc);
                    break;
                case WorkflowOrder.Created:
                    query = query.OrderBy(x => x.CreatedUtc);
                    break;
                default:
                    query = query.OrderByDescending(x => x.CreatedUtc);
                    break;
            }

            var pager = new Pager(pagerParameters, _pagerOptions.PageSize);

            var routeData = new RouteData();
            routeData.Values.Add("Filter", model.Options.Filter);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(await query.CountAsync()).RouteData(routeData);
            var pageOfItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();

            var viewModel = new WorkflowIndexViewModel
            {
                WorkflowType = workflowType,
                Workflows = pageOfItems.Select(x => new WorkflowEntry
                {
                    Workflow = x,
                    Id = x.Id
                }).ToList(),
                Options = model.Options,
                Pager = pagerShape,
                ReturnUrl = returnUrl
            };

            model.Options.WorkflowsSorts = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Recently created"], Value = nameof(WorkflowOrder.CreatedDesc) },
                new SelectListItem() { Text = S["Least recently created"], Value = nameof(WorkflowOrder.Created) }
            };

            model.Options.WorkflowsStatuses = new List<SelectListItem>() {
                new SelectListItem() { Text = S["All"], Value = nameof(WorkflowFilter.All) },
                new SelectListItem() { Text = S["Faulted"], Value = nameof(WorkflowFilter.Faulted) },
                new SelectListItem() { Text = S["Finished"], Value = nameof(WorkflowFilter.Finished) }
            };

            viewModel.Options.WorkflowsBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Delete"], Value = nameof(WorkflowBulkAction.Delete) }
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(WorkflowIndexViewModel model)
        {
            return RedirectToAction(nameof(Index), new RouteValueDictionary {
                { "Options.Filter", model.Options.Filter },
                { "Options.OrderBy", model.Options.OrderBy }
            });
        }

        public async Task<IActionResult> Details(int id)
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
                X = x.ActivityRecord.X,
                Y = x.ActivityRecord.Y,
                Name = x.ActivityRecord.Name,
                IsStart = x.ActivityRecord.IsStart,
                IsEvent = x.Activity.IsEvent(),
                IsBlocking = workflow.BlockingActivities.Any(a => a.ActivityId == x.ActivityRecord.ActivityId),
                Outcomes = x.Activity.GetPossibleOutcomes(workflowContext, x).ToArray()
            });
            var workflowTypeData = new
            {
                Id = workflowType.Id,
                Name = workflowType.Name,
                IsEnabled = workflowType.IsEnabled,
                Activities = activitiesDataQuery.ToArray(),
                Transitions = workflowType.Transitions
            };

            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var viewModel = new WorkflowViewModel
            {
                Workflow = workflow,
                WorkflowType = workflowType,
                WorkflowTypeJson = JsonConvert.SerializeObject(workflowTypeData, Formatting.None, jsonSerializerSettings),
                WorkflowJson = JsonConvert.SerializeObject(workflow, Formatting.Indented, jsonSerializerSettings),
                ActivityDesignShapes = activityDesignShapes
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
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
        public async Task<IActionResult> BulkEdit(int workflowTypeId, WorkflowIndexOptions options, PagerParameters pagerParameters, IEnumerable<int> itemIds)
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
                        throw new ArgumentOutOfRangeException();
                }
            }
            return RedirectToAction(nameof(Index), new { workflowTypeId, pagenum = pagerParameters.Page, pagesize = pagerParameters.PageSize });
        }

        private async Task<dynamic> BuildActivityDisplayAsync(ActivityContext activityContext, int workflowTypeId, bool isBlocking, string displayType)
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
