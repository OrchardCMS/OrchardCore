using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            IAuthorizationService authorizationService,
            IWorkflowManager workflowManager,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IWorkflowInstanceStore workflowInstanceStore,
            ISecurityTokenService securityTokenService,
            ILogger<WorkflowController> logger
        )
        {
            _authorizationService = authorizationService;
            _workflowManager = workflowManager;
            _workflowDefinitionStore = workflowDefinitionStore;
            _workflowInstanceStore = workflowInstanceStore;
            _securityTokenService = securityTokenService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateUrl(int workflowDefinitionId, string activityId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _workflowDefinitionStore.GetAsync(workflowDefinitionId);

            if (workflowDefinition == null)
            {
                return NotFound();
            }

            var token = _securityTokenService.CreateToken(new WorkflowPayload(workflowDefinition.WorkflowDefinitionId, activityId));
            var url = Url.Action("Invoke", "Workflow", new { token = token });

            return Ok(url);
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Invoke(string token)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ExecuteWorkflows))
            {
                return Unauthorized();
            }

            var result = _securityTokenService.DecryptToken<WorkflowPayload>(token);

            if (!result.IsSuccess)
            {
                _logger.LogDebug("Invalid SAS token provided: " + token);
                return NotFound();
            }

            var workflowDefinition = await _workflowDefinitionStore.GetAsync(result.Value.WorkflowId);

            if (workflowDefinition == null)
            {
                return NotFound();
            }

            var startActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == result.Value.ActivityId);

            if (startActivity.IsStart)
            {
                await _workflowManager.StartWorkflowAsync(workflowDefinition, startActivity);
            }
            else
            {
                var workflowInstances = await _workflowInstanceStore.ListAsync(workflowDefinition.WorkflowDefinitionId, new[] { startActivity.ActivityId });

                foreach (var workflowInstance in workflowInstances)
                {
                    var blockingActivity = workflowInstance.BlockingActivities.FirstOrDefault(x => x.ActivityId == startActivity.ActivityId);

                    if (blockingActivity != null)
                    {
                        await _workflowManager.ResumeWorkflowAsync(workflowInstance, blockingActivity);
                    }
                }
            }

            return new EmptyResult();
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Trigger(string token)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ExecuteWorkflows))
            {
                return Unauthorized();
            }

            var result = _securityTokenService.DecryptToken<SignalPayload>(token);

            if (!result.IsSuccess)
            {
                _logger.LogDebug("Invalid SAS token provided: " + token);
                return NotFound();
            }

            var input = new Dictionary<string, object> { { "Signal", result.Value.SignalName } };

            CopyTo(Request.Query, input);

            // If a specific workflow instance was provided, then resume that workflow instance.
            if (!string.IsNullOrWhiteSpace(result.Value.WorkflowInstanceId))
            {
                var workflowInstance = await _workflowInstanceStore.GetAsync(result.Value.WorkflowInstanceId);
                var signalActivity = workflowInstance?.BlockingActivities.FirstOrDefault(x => x.Name == SignalEvent.EventName);

                if (signalActivity == null)
                {
                    return NotFound();
                }

                // Resume the workflow instance.
                await _workflowManager.ResumeWorkflowAsync(workflowInstance, signalActivity, input);
            }
            else
            {
                // Resume all workflows with the specified correlation ID and start workflows with SignalEvent as their start activity.
                await _workflowManager.TriggerEventAsync(SignalEvent.EventName, input, result.Value.CorrelationId);
            }

            return new EmptyResult();
        }

        private void CopyTo(IEnumerable<KeyValuePair<string, StringValues>> source, IDictionary<string, object> target)
        {
            foreach (var item in source)
            {
                target[item.Key] = item.Value.ToString();
            }
        }
    }
}
