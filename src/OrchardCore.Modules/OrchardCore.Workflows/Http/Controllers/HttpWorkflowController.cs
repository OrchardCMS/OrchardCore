using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
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
        private readonly IActivityLibrary _activityLibrary;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly IAntiforgery _antiforgery;
        private readonly ILogger<HttpWorkflowController> _logger;

        public HttpWorkflowController(
            IAuthorizationService authorizationService,
            IWorkflowManager workflowManager,
            IWorkflowTypeStore workflowTypeStore,
            IWorkflowStore workflowStore,
            IActivityLibrary activityLibrary,
            ISecurityTokenService securityTokenService,
            IAntiforgery antiforgery,
            ILogger<HttpWorkflowController> logger
        )
        {
            _authorizationService = authorizationService;
            _workflowManager = workflowManager;
            _workflowTypeStore = workflowTypeStore;
            _workflowStore = workflowStore;
            _activityLibrary = activityLibrary;
            _securityTokenService = securityTokenService;
            _antiforgery = antiforgery;
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

            // Get the workflow type.
            var workflowType = await _workflowTypeStore.GetAsync(payload.WorkflowId);

            if (workflowType == null)
            {
                _logger.LogWarning("The provided workflow type with ID '{WorkflowTypeId}' could not be found.", payload.WorkflowId);
                return NotFound();
            }

            // Get the activity record using the activity ID provided by the token.
            var startActivity = workflowType.Activities.FirstOrDefault(x => x.ActivityId == payload.ActivityId);

            if (startActivity == null)
            {
                _logger.LogWarning("The provided activity with ID '{ActivityId}' could not be found.", payload.ActivityId);
                return NotFound();
            }

            // Instantiate and bind an actual HttpRequestEvent object to check its settings.
            var httpRequestActivity = _activityLibrary.InstantiateActivity<HttpRequestEvent>(startActivity);

            if (httpRequestActivity == null)
            {
                _logger.LogWarning("Activity with name '{ActivityName}' could not be found.", startActivity.Name);
                return NotFound();
            }

            // Check if the HttpRequestEvent is configured to perform antiforgery token validation. If so, perform the validation.
            if (httpRequestActivity.ValidateAntiforgeryToken && (!await _antiforgery.IsRequestValidAsync(HttpContext)))
            {
                _logger.LogWarning("Antiforgery token validation failed.");
                return BadRequest();
            }

            // If the activity is a start activity, start a new workflow.
            if (startActivity.IsStart)
            {
                _logger.LogDebug("Invoking new workflow of type {WorkflowTypeId} with start activity {ActivityId}", workflowType.WorkflowTypeId, startActivity.ActivityId);
                await _workflowManager.StartWorkflowAsync(workflowType, startActivity);
            }
            else
            {
                // Otherwise, we need to resume a halted workflow.
                var workflow = (await _workflowStore.ListAsync(workflowType.WorkflowTypeId, new[] { startActivity.ActivityId })).FirstOrDefault();

                if (workflow == null)
                {
                    _logger.LogWarning("No workflow found that is blocked on activity {ActivityId}", startActivity.ActivityId);
                    return NotFound();
                }

                var blockingActivity = workflow.BlockingActivities.FirstOrDefault(x => x.ActivityId == startActivity.ActivityId);

                if (blockingActivity != null)
                {
                    _logger.LogDebug("Resuming workflow with ID {WorkflowId} on activity {ActivityId}", workflow.WorkflowId, blockingActivity.ActivityId);
                    await _workflowManager.ResumeWorkflowAsync(workflow, blockingActivity);
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
            if (HttpContext.Items.ContainsKey(WorkflowHttpResult.Instance))
            {
                return new EmptyResult();
            }

            return Accepted();
        }
    }
}
