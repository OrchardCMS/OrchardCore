using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            IAuthorizationService authorizationService,
            IWorkflowManager workflowManager,
            IWorkflowDefinitionRepository workflowDefinitionRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            ISecurityTokenService securityTokenService,
            ILogger<WorkflowController> logger
        )
        {
            _authorizationService = authorizationService;
            _workflowManager = workflowManager;
            _workflowDefinitionRepository = workflowDefinitionRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
            _securityTokenService = securityTokenService;
            _logger = logger;
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Start(string token)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ExecuteWorkflows))
            {
                return Unauthorized();
            }

            var result = _securityTokenService.DecryptToken<StartResumeWorkflowPayload>(token);

            if (!result.IsSuccess)
            {
                _logger.LogDebug("Invalid SAS token provided: " + token);
                return NotFound();
            }

            var workflowDefinition = await _workflowDefinitionRepository.GetAsync(result.Value.WorkflowId);

            if (workflowDefinition == null)
            {
                return NotFound();
            }

            var activity = workflowDefinition.Activities.SingleOrDefault(x => x.Id == result.Value.ActivityId);
            await _workflowManager.StartWorkflowAsync(workflowDefinition, activity);

            return new EmptyResult();
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Resume(string token)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ExecuteWorkflows))
            {
                return Unauthorized();
            }

            var result = _securityTokenService.DecryptToken<StartResumeWorkflowPayload>(token);

            if (!result.IsSuccess)
            {
                _logger.LogDebug("Invalid SAS token provided: " + token);
                return NotFound();
            }

            var workflowInstance = await _workflowInstanceRepository.GetAsync(result.Value.WorkflowId);

            if (workflowInstance == null)
            {
                return NotFound();
            }

            var activity = workflowInstance.AwaitingActivities.Single(x => x.ActivityId == result.Value.ActivityId);
            await _workflowManager.ResumeWorkflowAsync(workflowInstance, activity);

            return new EmptyResult();
        }
    }
}
