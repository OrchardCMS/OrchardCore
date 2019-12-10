using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.ReCaptcha.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.ReCaptcha.Workflows
{
    public class ValidateReCaptchaTask : TaskActivity
    {
        private readonly ReCaptchaService _reCaptchaService;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public ValidateReCaptchaTask(
            ReCaptchaService reCaptchaService,
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<ValidateReCaptchaTask> localizer
        )
        {
            _reCaptchaService = reCaptchaService;
            _updateModelAccessor = updateModelAccessor;
            T = localizer;
        }

        public override string Name => nameof(ValidateReCaptchaTask);
        public override LocalizedString DisplayText => T["Validate ReCaptcha Task"];
        public override LocalizedString Category => T["Validation"];
        public override bool HasEditor => false;

        private IStringLocalizer T { get; set; }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"], T["Valid"], T["Invalid"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var outcome = "Valid";

            await _reCaptchaService.ValidateCaptchaAsync((key, error) =>
            {
                var updater = _updateModelAccessor.ModelUpdater;
                outcome = "Invalid";
                if (updater != null)
                {
                    updater.ModelState.TryAddModelError(Constants.ReCaptchaServerResponseHeaderName, T["Captcha validation failed. Try again."]);
                }
            });

            return Outcomes("Done", outcome);
        }
    }
}