using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.Services;
using OrchardCore.Modules;

namespace OrchardCore.Forms.Controllers
{
    public class SubmissionController : Controller
    {
        private readonly IEnumerable<IFormHandler> _handlers;
        private readonly IContentManager _contentManager;
        private readonly ILogger<SubmissionController> _logger;

        public SubmissionController(IEnumerable<IFormHandler> handlers, IContentManager contentManager, ILogger<SubmissionController> logger)
        {
            _handlers = handlers;
            _contentManager = contentManager;
            _logger = logger;
        }

        public async Task<IActionResult> Post(string formId)
        {
            var context = new FormSubmittedContext(formId, Request.Form, async () => await _contentManager.GetAsync(formId, VersionOptions.Published));
            await _handlers.InvokeAsync(async handler => await handler.SubmittedAsync(context), _logger);
            return GetFormActionResult();
        }

        /// <summary>
        /// Returns the appropriate action result depending on whether the status code has already been set by a form handler.
        /// </summary>
        private IActionResult GetFormActionResult()
        {
            if (Response.StatusCode != 0 && Response.StatusCode != (int)HttpStatusCode.OK)
            {
                return new EmptyResult();
            }

            return Accepted();
        }
    }
}
