using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Forms.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities
{
    public class ValidateNoCaptchaTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly NoCaptchaClient _noCaptchaClient;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public ValidateNoCaptchaTask(
            IHttpContextAccessor httpContextAccessor,
            NoCaptchaClient noCaptchaClient,
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<ValidateNoCaptchaTask> localizer
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _noCaptchaClient = noCaptchaClient;
            _updateModelAccessor = updateModelAccessor;
            T = localizer;
        }

        public override string Name => nameof(ValidateNoCaptchaTask);
        public override LocalizedString Category => T["Validation"];
        public override bool HasEditor => false;

        private IStringLocalizer T { get; set; }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"], T["Valid"], T["Invalid"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var captchaResponse = httpContext.Request.Form["g-recaptcha-response"];
            var isValid = await _noCaptchaClient.VerifyAsync(captchaResponse);
            var outcome = isValid ? "Valid" : "Invalid";

            if (!isValid)
            {
                var updater = _updateModelAccessor.ModelUpdater;

                if (updater != null)
                {
                    updater.ModelState.TryAddModelError("g-recaptcha-response", T["Captcha validation failed. Try again."]);
                }
            }

            return Outcomes("Done", outcome);
        }
    }
}