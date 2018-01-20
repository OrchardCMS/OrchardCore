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
    public class WorkflowInstanceController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IActivityDisplayManager _activityDisplayManager;
        private readonly INotifier _notifier;
        private readonly ILogger<WorkflowInstanceController> _logger;

        public WorkflowInstanceController(
            ISiteService siteService,
            IWorkflowManager workflowManager,
            IWorkflowDefinitionRepository workflowDefinitionRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IAuthorizationService authorizationService,
            IActivityDisplayManager activityDisplayManager,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IHtmlLocalizer<WorkflowInstanceController> localizer,
            ILogger<WorkflowInstanceController> logger
        )
        {
            _siteService = siteService;
            _workflowManager = workflowManager;
            _workflowDefinitionRepository = workflowDefinitionRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
            _authorizationService = authorizationService;
            _activityDisplayManager = activityDisplayManager;
            _notifier = notifier;
            _logger = logger;

            New = shapeFactory;
            T = localizer;
        }

        private dynamic New { get; }
        private IHtmlLocalizer<WorkflowInstanceController> T { get; }

        public async Task<IActionResult> Index(int workflowDefinitionId, PagerParameters pagerParameters, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Action(nameof(Index), "WorkflowDefinition");
            }

            var workflowDefinition = await _workflowDefinitionRepository.GetAsync(workflowDefinitionId);
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var count = await _workflowInstanceRepository.CountAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var records = await _workflowInstanceRepository.ListAsync(pager.GetStartIndex(), pager.PageSize);
            var pagerShape = (await New.Pager(pager)).TotalItemCount(count);

            var viewModel = new WorkflowInstanceIndexViewModel
            {
                WorkflowDefinition = workflowDefinition,
                WorkflowInstances = records.Select(x => new WorkflowInstanceEntry
                {
                    WorkflowInstance = x,
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

            var workflowInstance = await _workflowInstanceRepository.GetAsync(id);

            if (workflowInstance == null)
            {
                return NotFound();
            }

            var workflowDefinitionRecord = await _workflowDefinitionRepository.GetAsync(workflowInstance.DefinitionId);
            var blockingActivities = workflowInstance.AwaitingActivities.ToDictionary(x => x.ActivityId);
            var workflowContext = await _workflowManager.CreateWorkflowExecutionContextAsync(workflowDefinitionRecord, workflowInstance);
            var activityContexts = await Task.WhenAll(workflowDefinitionRecord.Activities.Select(async x => await _workflowManager.CreateActivityExecutionContextAsync(x)));
            var activityDesignShapes = (await Task.WhenAll(activityContexts.Select(async x => await BuildActivityDisplayAsync(x, workflowDefinitionRecord.Id, blockingActivities.ContainsKey(x.ActivityRecord.Id), "Design")))).ToList();
            var activitiesDataQuery = activityContexts.Select(x => new
            {
                Id = x.ActivityRecord.Id,
                X = x.ActivityRecord.X,
                Y = x.ActivityRecord.Y,
                Name = x.ActivityRecord.Name,
                IsStart = x.ActivityRecord.IsStart,
                IsEvent = x.Activity.IsEvent(),
                IsBlocking = workflowInstance.AwaitingActivities.Any(a => a.ActivityId == x.ActivityRecord.Id),
                Outcomes = x.Activity.GetPossibleOutcomes(workflowContext, x).ToArray()
            });
            var workflowDefinitionData = new
            {
                Id = workflowDefinitionRecord.Id,
                Name = workflowDefinitionRecord.Name,
                IsEnabled = workflowDefinitionRecord.IsEnabled,
                Activities = activitiesDataQuery.ToArray(),
                Transitions = workflowDefinitionRecord.Transitions
            };

            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var viewModel = new WorkflowInstanceViewModel
            {
                WorkflowInstance = workflowInstance,
                WorkflowDefinition = workflowDefinitionRecord,
                WorkflowDefinitionJson = JsonConvert.SerializeObject(workflowDefinitionData, Formatting.None, jsonSerializerSettings),
                WorkflowInstanceJson = JsonConvert.SerializeObject(workflowInstance, Formatting.Indented, jsonSerializerSettings),
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

            var workflowInstance = await _workflowInstanceRepository.GetAsync(id);

            if (workflowInstance == null)
            {
                _notifier.Information(T["Workflow instance {0} no longer exists.", id]);
                return RedirectToAction("Index", "WorkflowDefinition");
            }
            else
            {
                await _workflowInstanceRepository.DeleteAsync(workflowInstance);
                _notifier.Success(T["Workflow instance {0} has been deleted.", id]);
                return RedirectToAction("Index", new { workflowDefinitionId = workflowInstance.DefinitionId });
            }
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        [FormValueRequired("BulkAction")]
        public async Task<IActionResult> BulkEdit(WorkflowInstanceBulkAction bulkAction, PagerParameters pagerParameters, int workflowDefinitionId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var viewModel = new WorkflowInstanceIndexViewModel { WorkflowInstances = new List<WorkflowInstanceEntry>() };

            if (!(await TryUpdateModelAsync(viewModel)))
            {
                return View(viewModel);
            }

            var checkedEntries = viewModel.WorkflowInstances.Where(t => t.IsChecked);
            switch (bulkAction)
            {
                case WorkflowInstanceBulkAction.None:
                    break;
                case WorkflowInstanceBulkAction.Delete:
                    foreach (var entry in checkedEntries)
                    {
                        var workflowInstance = await _workflowInstanceRepository.GetAsync(entry.Id);

                        if (workflowInstance != null)
                        {
                            await _workflowInstanceRepository.DeleteAsync(workflowInstance);
                            _notifier.Success(T["Workflow instance {0} has been deleted.", workflowInstance.Uid]);
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return RedirectToAction("Index", new { workflowDefinitionId, page = pagerParameters.Page, pageSize = pagerParameters.PageSize });
        }

        private async Task<dynamic> BuildActivityDisplayAsync(ActivityContext activityContext, int workflowDefinitionId, bool isBlocking, string displayType)
        {
            dynamic activityShape = await _activityDisplayManager.BuildDisplayAsync(activityContext.Activity, this, displayType);
            activityShape.Metadata.Type = $"Activity_{displayType}ReadOnly";
            activityShape.Activity = activityContext.Activity;
            activityShape.ActivityRecord = activityContext.ActivityRecord;
            activityShape.WorkflowDefinitionId = workflowDefinitionId;
            activityShape.IsBlocking = isBlocking;
            return activityShape;
        }
    }
}
