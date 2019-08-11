using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities
{
    // TODO: Add the ability to configure various types of validators.
    public class ValidateFormFieldTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public ValidateFormFieldTask(
            IHttpContextAccessor httpContextAccessor,
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<ValidateFormFieldTask> localizer
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _updateModelAccessor = updateModelAccessor;
            T = localizer;
        }

        public override string Name => nameof(ValidateFormFieldTask);
        public override LocalizedString Category => T["Validation"];

        private IStringLocalizer T { get; set; }

        public string FieldName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string ErrorMessage
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"], T["Valid"], T["Invalid"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var form = _httpContextAccessor.HttpContext.Request.Form;
            var fieldValue = form[FieldName];
            var isValid = !string.IsNullOrWhiteSpace(fieldValue);
            var outcome = isValid ? "Valid" : "Invalid";

            if (!isValid)
            {
                var updater = _updateModelAccessor.ModelUpdater;

                if (updater != null)
                {
                    updater.ModelState.TryAddModelError(FieldName, ErrorMessage);
                }
            }

            return Outcomes("Done", outcome);
        }
    }
}