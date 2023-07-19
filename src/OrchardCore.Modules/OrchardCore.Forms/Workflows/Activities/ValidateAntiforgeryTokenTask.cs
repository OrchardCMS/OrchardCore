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
    public class ValidateAntiforgeryTokenTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAntiforgery _antiforgery;
        protected readonly IStringLocalizer S;

        public ValidateAntiforgeryTokenTask(
            IHttpContextAccessor httpContextAccessor,
            IAntiforgery antiforgery,
            IStringLocalizer<ValidateAntiforgeryTokenTask> localizer
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _antiforgery = antiforgery;
            S = localizer;
        }

        public override string Name => nameof(ValidateAntiforgeryTokenTask);

        public override LocalizedString DisplayText => S["Validate Antiforgery Token Task"];

        public override LocalizedString Category => S["Validation"];

        public override bool HasEditor => false;

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Valid"], S["Invalid"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (await _antiforgery.IsRequestValidAsync(_httpContextAccessor.HttpContext))
            {
                return Outcomes("Done", "Valid");
            }
            else
            {
                return Outcomes("Done", "Invalid");
            }
        }
    }
}
