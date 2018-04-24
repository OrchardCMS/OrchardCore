using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Controllers
{
    public class HttpWorkflowController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly IWorkflowStore _workflowStore;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly ILogger<HttpWorkflowController> _logger;

        public HttpWorkflowController(
            IAuthorizationService authorizationService,
            IWorkflowManager workflowManager,
            IWorkflowTypeStore workflowTypeStore,
            IWorkflowStore workflowStore,
            ISecurityTokenService securityTokenService,
            ILogger<HttpWorkflowController> logger
        )
        {
            _authorizationService = authorizationService;
            _workflowManager = workflowManager;
            _workflowTypeStore = workflowTypeStore;
            _workflowStore = workflowStore;
            _securityTokenService = securityTokenService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateUrl(int workflowTypeId, string activityId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

            if (workflowType == null)
            {
                return NotFound();
            }

            var token = _securityTokenService.CreateToken(new WorkflowPayload(workflowType.WorkflowTypeId, activityId), TimeSpan.FromDays(7));
            var url = Url.Action("Invoke", "HttpWorkflow", new { token = token });

            return Ok(url);
        }

        [HttpGet]
        public async Task<IActionResult> Invoke(string token)
        {
            if (!_securityTokenService.TryDecryptToken<WorkflowPayload>(token, out var payload))
            {
                _logger.LogWarning("Invalid SAS token provided");
                return NotFound();
            }

            var workflowType = await _workflowTypeStore.GetAsync(payload.WorkflowId);

            if (workflowType == null)
            {
                return NotFound();
            }

            var startActivity = workflowType.Activities.FirstOrDefault(x => x.ActivityId == payload.ActivityId);

            if (startActivity.IsStart)
            {
                await _workflowManager.StartWorkflowAsync(workflowType, startActivity);
            }
            else
            {
                var workflows = await _workflowStore.ListAsync(workflowType.WorkflowTypeId, new[] { startActivity.ActivityId });

                foreach (var workflow in workflows)
                {
                    var blockingActivity = workflow.BlockingActivities.FirstOrDefault(x => x.ActivityId == startActivity.ActivityId);

                    if (blockingActivity != null)
                    {
                        await _workflowManager.ResumeWorkflowAsync(workflow, blockingActivity);
                    }
                }
            }

            return Accepted();
        }

        [HttpGet]
        public async Task<IActionResult> Trigger(string token)
        {
            if (!_securityTokenService.TryDecryptToken<SignalPayload>(token, out var payload))
            {
                _logger.LogWarning("Invalid SAS token provided");
                return NotFound();
            }

            var input = new Dictionary<string, object> { { "Signal", payload.SignalName } };

            CopyTo(Request.Query, input);

            // If a specific workflow instance was provided, then resume that workflow instance.
            if (!String.IsNullOrWhiteSpace(payload.WorkflowId))
            {
                var workflow = await _workflowStore.GetAsync(payload.WorkflowId);
                var signalActivity = workflow?.BlockingActivities.FirstOrDefault(x => x.Name == SignalEvent.EventName);

                if (signalActivity == null)
                {
                    return NotFound();
                }

                // Resume the workflow instance.
                await _workflowManager.ResumeWorkflowAsync(workflow, signalActivity, input);
            }
            else
            {
                // Resume all workflows with the specified correlation ID and start workflows with SignalEvent as their start activity.
                await _workflowManager.TriggerEventAsync(SignalEvent.EventName, input, payload.CorrelationId);
            }

            return Accepted();
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
