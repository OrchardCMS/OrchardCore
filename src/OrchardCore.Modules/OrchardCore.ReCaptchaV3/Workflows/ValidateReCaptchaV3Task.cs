using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.ReCaptchaV3.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.ReCaptchaV3.Workflows
{
    public class ValidateReCaptchaV3Task : TaskActivity
    {
        private readonly ReCaptchaV3Service _reCaptchaV3Service;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IStringLocalizer S;

        public ValidateReCaptchaV3Task(
            ReCaptchaV3Service reCaptchaV3Service,
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<ValidateReCaptchaV3Task> localizer
        )
        {
            _reCaptchaV3Service = reCaptchaV3Service;
            _updateModelAccessor = updateModelAccessor;
            S = localizer;
        }

        public override string Name => nameof(ValidateReCaptchaV3Task);

        public override LocalizedString DisplayText => S["Validate ReCaptchaV3 Task"];

        public override LocalizedString Category => S["Validation"];

        public override bool HasEditor => false;

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Valid"], S["Invalid"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var outcome = "Valid";

            await _reCaptchaV3Service.ValidateCaptchaV3Async((key, error) =>
            {
                var updater = _updateModelAccessor.ModelUpdater;
                outcome = "Invalid";
                if (updater != null)
                {
                    updater.ModelState.TryAddModelError(Constants.ReCaptchaV3ServerResponseHeaderName, S["Captcha V3 validation failed."]);
                }
            });

            return Outcomes("Done", outcome);
        }
    }
}
