using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Controllers
{
    public class SignalController : Controller
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        private readonly ISecurityTokenService _securitytokenService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<SignalController> _logger;

        public SignalController(
            IWorkflowManager workflowManager,
            IWorkflowInstanceRepository workflowInstanceRepository,
            ISecurityTokenService securitytokenService,
            IAuthorizationService authenticationService,
            ILogger<SignalController> logger
        )
        {
            _workflowManager = workflowManager;
            _workflowInstanceRepository = workflowInstanceRepository;
            _securitytokenService = securitytokenService;
            _authorizationService = authenticationService;
            _logger = logger;
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Trigger(string token)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ExecuteWorkflows))
            {
                return Unauthorized();
            }

            var result = _securitytokenService.DecryptToken<SignalPayload>(token);

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
                var workflowInstance = await _workflowInstanceRepository.GetAsync(result.Value.WorkflowInstanceId);
                var signalActivity = workflowInstance?.AwaitingActivities.FirstOrDefault(x => x.Name == SignalEvent.EventName);

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
