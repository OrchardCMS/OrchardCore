using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            IAuthorizationService authorizationService,
            IWorkflowManager workflowManager,
            IWorkflowDefinitionRepository workflowDefinitionRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            ILogger<WorkflowController> logger)
        {
            _authorizationService = authorizationService;
            _workflowManager = workflowManager;
            _workflowDefinitionRepository = workflowDefinitionRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Start(int workflowDefinitionId, int activityId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ExecuteWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _workflowDefinitionRepository.GetAsync(workflowDefinitionId);

            if (workflowDefinition == null)
            {
                return NotFound();
            }

            var activity = workflowDefinition.Activities.Single(x => x.Id == activityId);
            await _workflowManager.StartWorkflowAsync(workflowDefinition, activity);

            return new EmptyResult();
        }

        [HttpPost]
        public async Task<IActionResult> Resume(string uid, int activityId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ExecuteWorkflows))
            {
                return Unauthorized();
            }

            var workflowInstance = await _workflowInstanceRepository.GetAsync(uid);

            if (workflowInstance == null)
            {
                return NotFound();
            }

            var activity = workflowInstance.AwaitingActivities.Single(x => x.ActivityId == activityId);
            await _workflowManager.ResumeWorkflowAsync(workflowInstance, activity);

            return new EmptyResult();
        }
    }
}
