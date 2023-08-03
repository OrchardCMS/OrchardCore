using System.Collections.Generic;
using System.Threading.Tasks;
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
        protected readonly IStringLocalizer S;

        public ValidateReCaptchaTask(
            ReCaptchaService reCaptchaService,
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<ValidateReCaptchaTask> localizer
        )
        {
            _reCaptchaService = reCaptchaService;
            _updateModelAccessor = updateModelAccessor;
            S = localizer;
        }

        public override string Name => nameof(ValidateReCaptchaTask);

        public override LocalizedString DisplayText => S["Validate ReCaptcha Task"];

        public override LocalizedString Category => S["Validation"];

        public override bool HasEditor => false;

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Valid"], S["Invalid"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var outcome = "Valid";

            await _reCaptchaService.ValidateCaptchaAsync((key, error) =>
            {
                var updater = _updateModelAccessor.ModelUpdater;
                outcome = "Invalid";

                updater?.ModelState.TryAddModelError(Constants.ReCaptchaServerResponseHeaderName, S["Captcha validation failed. Try again."]);
            });

            return Outcomes("Done", outcome);
        }
    }
}
