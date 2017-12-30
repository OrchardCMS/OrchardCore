using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Controllers
{
    public class SignalController : Controller
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly ISignalService _signalService;
        private readonly ILogger<SignalController> _logger;

        public SignalController(IWorkflowManager workflowManager, ISignalService signalService, ILogger<SignalController> logger)
        {
            _workflowManager = workflowManager;
            _signalService = signalService;
            _logger = logger;
        }

        public async Task<IActionResult> Trigger(string nonce)
        {
            string correlationId;
            string signal;

            if (!_signalService.DecryptNonce(nonce, out correlationId, out signal))
            {
                _logger.LogDebug("Invalid nonce provided: " + nonce);
                return NotFound();
            }

            var input = new Dictionary<string, object> { { "Signal", signal } };

            // Include request details, so that the Signal activity can decide whether or not it should execute.
            CopyTo(Request.Query, input);
            CopyTo(Request.Form, input);
            CopyTo(Request.Headers, input);

            await _workflowManager.TriggerEventAsync(SignalEvent.EventName, input, correlationId);

            return NoContent();
        }

        private void CopyTo(IEnumerable<KeyValuePair<string, StringValues>> source, IDictionary<string, object> target)
        {
            foreach (var item in source)
            {
                target[item.Key] = item.Value;
            }
        }
    }
}
