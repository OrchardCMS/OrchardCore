using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        [IgnoreAntiforgeryToken]
        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpPatch]
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

            return GetWorkflowActionResult();
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

            // If a specific workflow was provided, then resume that workflow.
            if (!String.IsNullOrWhiteSpace(payload.WorkflowId))
            {
                var workflow = await _workflowStore.GetAsync(payload.WorkflowId);
                var signalActivities = workflow?.BlockingActivities.Where(x => x.Name == SignalEvent.EventName).ToList();

                if (signalActivities == null)
                {
                    return NotFound();
                }

                // The workflow could be blocking on multiple Signal activities, but only the activity with the provided signal name
                // will be executed as SignalEvent checks for the provided "Signal" input.
                foreach (var signalActivity in signalActivities)
                {
                    await _workflowManager.ResumeWorkflowAsync(workflow, signalActivity, input);
                }
            }
            else
            {
                // Resume all workflows with the specified correlation ID and start workflows with SignalEvent as their start activity.
                await _workflowManager.TriggerEventAsync(SignalEvent.EventName, input, payload.CorrelationId);
            }

            return GetWorkflowActionResult();
        }

        /// <summary>
        /// Returns the appropriate action result depending on whether the status code has already been set by a workflow.
        /// </summary>
        private IActionResult GetWorkflowActionResult()
        {
            if (Response.StatusCode != 0 && Response.StatusCode != (int)HttpStatusCode.OK)
            {
                return new EmptyResult();
            }

            return Accepted();
        }
    }
}
