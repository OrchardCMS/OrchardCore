using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Forms.Extensions;
using OrchardCore.Forms.Models;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities
{
    public class ValidateRuleFormTask : TaskActivity
    {
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStringLocalizer S;

        public ValidateRuleFormTask(
            IHttpContextAccessor httpContextAccessor,
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<ValidateRuleFormTask> localizer
        )
        {
            _updateModelAccessor = updateModelAccessor;
            _httpContextAccessor = httpContextAccessor;
            S = localizer;
        }

        public override string Name => nameof(ValidateRuleFormTask);

        public override LocalizedString DisplayText => S["Validate Rule Form Task"];

        public override LocalizedString Category => S["Validation"];

        public override bool HasEditor => false;

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Valid"], S["Invalid"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var updater = _updateModelAccessor.ModelUpdater;

            if (updater == null)
            {
                throw new InvalidOperationException("Cannot add model validation errors when there's no Updater present.");
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var validateRule = true;
            var rules = new List<ValidationRuleModel>();
            foreach (var item in httpContext.Request.Form)
            {
                if (item.Key.Equals("validationRules", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!String.IsNullOrEmpty(item.Value[0]))
                    {
                        rules = JsonConvert.DeserializeObject<List<ValidationRuleModel>>(item.Value[0]);
                    }
                }
                else
                {
                    updater.ModelState.SetModelValue(item.Key, item.Value, item.Value);
                }
            }

            if (rules.Count > 0)
            {
                foreach (var item in rules)
                {
                    if (item.Type != "none")
                    {
                        if (item.Option.Contains("\\", StringComparison.Ordinal))
                        {
                            item.Option = item.Option.Replace("\\", "|-BackslashPlaceholder-|");
                        }
                        var formItemValue = httpContext.Request.Form[item.ElementId];
                        if (!item.Type.ValidateInputByRule(formItemValue, item.Option))
                        {
                            validateRule = false;
                            break;
                        }
                    }
                }
            }

            var isValid = updater.ModelState.ErrorCount == 0;
            var outcome = isValid && validateRule ? "Valid" : "Invalid";
            return Outcomes(outcome);
        }
    }
}
