using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.ActionConstraints;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Controllers
{
    [Admin]
    public class WorkflowController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly IWorkflowStore _workflowStore;
        private readonly IAuthorizationService _authorizationService;
        private readonly IActivityDisplayManager _activityDisplayManager;
        private readonly INotifier _notifier;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            ISiteService siteService,
            IWorkflowManager workflowManager,
            IWorkflowTypeStore workflowTypeStore,
            IWorkflowStore workflowStore,
            IAuthorizationService authorizationService,
            IActivityDisplayManager activityDisplayManager,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IHtmlLocalizer<WorkflowController> localizer,
            ILogger<WorkflowController> logger
        )
        {
            _siteService = siteService;
            _workflowManager = workflowManager;
            _workflowTypeStore = workflowTypeStore;
            _workflowStore = workflowStore;
            _authorizationService = authorizationService;
            _activityDisplayManager = activityDisplayManager;
            _notifier = notifier;
            _logger = logger;

            New = shapeFactory;
            T = localizer;
        }

        private dynamic New { get; }
        private IHtmlLocalizer<WorkflowController> T { get; }

        public async Task<IActionResult> Index(int workflowTypeId, PagerParameters pagerParameters, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Action(nameof(Index), "WorkflowType");
            }

            var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var count = await _workflowStore.CountAsync(workflowType.WorkflowTypeId);
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var records = await _workflowStore.ListAsync(workflowType.WorkflowTypeId, pager.GetStartIndex(), pager.PageSize);
            var pagerShape = (await New.Pager(pager)).TotalItemCount(count);

            var viewModel = new WorkflowIndexViewModel
            {
                WorkflowType = workflowType,
                Workflows = records.Select(x => new WorkflowEntry
                {
                    Workflow = x,
                    Id = x.Id
                }).ToList(),
                Pager = pagerShape,
                ReturnUrl = returnUrl
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflow = await _workflowStore.GetAsync(id);

            if (workflow == null)
            {
                return NotFound();
            }

            var workflowType = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);
            var blockingActivities = workflow.BlockingActivities.ToDictionary(x => x.ActivityId);
            var workflowContext = await _workflowManager.CreateWorkflowExecutionContextAsync(workflowType, workflow);
            var activityContexts = await Task.WhenAll(workflowType.Activities.Select(async x => await _workflowManager.CreateActivityExecutionContextAsync(x, x.Properties)));
            var activityDesignShapes = (await Task.WhenAll(activityContexts.Select(async x => await BuildActivityDisplayAsync(x, workflowType.Id, blockingActivities.ContainsKey(x.ActivityRecord.ActivityId), "Design")))).ToList();
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
                return Unauthorized();
            }

            var workflow = await _workflowStore.GetAsync(id);

            if (workflow == null)
            {
                return NotFound();
            }

            var workflowType = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);
            await _workflowStore.DeleteAsync(workflow);
            _notifier.Success(T["Workflow {0} has been deleted.", id]);
            return RedirectToAction("Index", new { workflowTypeId = workflowType.Id });
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        [FormValueRequired("BulkAction")]
        public async Task<IActionResult> BulkEdit(WorkflowBulkAction bulkAction, PagerParameters pagerParameters, int workflowTypeId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var viewModel = new WorkflowIndexViewModel { Workflows = new List<WorkflowEntry>() };

            if (!(await TryUpdateModelAsync(viewModel)))
            {
                return View(viewModel);
            }

            var checkedEntries = viewModel.Workflows.Where(t => t.IsChecked);
            switch (bulkAction)
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
                            _notifier.Success(T["Workflow {0} has been deleted.", workflow.WorkflowId]);
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return RedirectToAction("Index", new { workflowTypeId, page = pagerParameters.Page, pageSize = pagerParameters.PageSize });
        }

        private async Task<dynamic> BuildActivityDisplayAsync(ActivityContext activityContext, int workflowTypeId, bool isBlocking, string displayType)
        {
            dynamic activityShape = await _activityDisplayManager.BuildDisplayAsync(activityContext.Activity, this, displayType);
            activityShape.Metadata.Type = $"Activity_{displayType}ReadOnly";
            activityShape.Activity = activityContext.Activity;
            activityShape.ActivityRecord = activityContext.ActivityRecord;
            activityShape.WorkflowTypeId = workflowTypeId;
            activityShape.IsBlocking = isBlocking;
            return activityShape;
        }
    }
}
