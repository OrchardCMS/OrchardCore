using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Workflows.Activities;
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
        private readonly IDisplayManager<IActivity> _activityDisplayManager;
        private readonly ILogger<WorkflowInstanceController> _logger;

        public WorkflowInstanceController(
            ISiteService siteService,
            IWorkflowManager workflowManager,
            IWorkflowDefinitionRepository workflowDefinitionRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IAuthorizationService authorizationService,
            IDisplayManager<IActivity> activityDisplayManager,
            IShapeFactory shapeFactory,
            ILogger<WorkflowInstanceController> logger
        )
        {
            _siteService = siteService;
            _workflowManager = workflowManager;
            _workflowDefinitionRepository = workflowDefinitionRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
            _authorizationService = authorizationService;
            _activityDisplayManager = activityDisplayManager;
            _logger = logger;

            New = shapeFactory;
        }

        private dynamic New { get; }

        public async Task<IActionResult> Index(int workflowDefinitionId, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
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
                }).ToList(),
                Pager = pagerShape
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
            var workflowDefinitionRecord = await _workflowDefinitionRepository.GetAsync(workflowInstance.DefinitionId);
            var workflowContext = _workflowManager.CreateWorkflowContext(workflowDefinitionRecord, workflowInstance);
            var activityContexts = workflowDefinitionRecord.Activities.Select(x => _workflowManager.CreateActivityContext(x)).ToList();
            var activityDesignShapes = (await Task.WhenAll(activityContexts.Select(async x => await BuildActivityDisplayAsync(x, workflowDefinitionRecord.Id, "Design")))).ToList();
            var activitiesDataQuery = activityContexts.Select(x => new
            {
                Id = x.ActivityRecord.Id,
                X = x.ActivityRecord.X,
                Y = x.ActivityRecord.Y,
                Name = x.ActivityRecord.Name,
                IsStart = x.ActivityRecord.IsStart,
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
            var viewModel = new WorkflowInstanceViewModel
            {
                WorkflowInstance = workflowInstance,
                WorkflowDefinition = workflowDefinitionRecord,
                WorkflowDefinitionJson = JsonConvert.SerializeObject(workflowDefinitionData, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                ActivityDesignShapes = activityDesignShapes
            };
            return View(viewModel);
        }

        private async Task<dynamic> BuildActivityDisplayAsync(ActivityContext activityContext, int workflowDefinitionId, string displayType)
        {
            dynamic activityShape = await _activityDisplayManager.BuildDisplayAsync(activityContext.Activity, this, displayType);
            activityShape.Metadata.Type = $"Activity_{displayType}ReadOnly";
            activityShape.Activity = activityContext.Activity;
            activityShape.ActivityRecord = activityContext.ActivityRecord;
            activityShape.WorkflowDefinitionId = workflowDefinitionId;
            return activityShape;
        }
    }
}
