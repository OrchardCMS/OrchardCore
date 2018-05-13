using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities
{
    public class ValidateCaptchaTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ValidateCaptchaTask(
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<ValidateCaptchaTask> localizer
        )
        {
            _httpContextAccessor = httpContextAccessor;
            T = localizer;
        }

        public override string Name => nameof(ValidateCaptchaTask);
        public override LocalizedString Category => T["Validation"];
        public override bool HasEditor => false;

        private IStringLocalizer T { get; set; }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Valid"], T["Invalid"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (await _antiforgery.IsRequestValidAsync(_httpContextAccessor.HttpContext))
            {
                return Outcomes("Valid");
            }
            else
            {
                return Outcomes("Invalid");
            }
        }
    }
}