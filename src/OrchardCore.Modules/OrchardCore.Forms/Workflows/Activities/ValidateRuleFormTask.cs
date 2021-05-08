using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Forms.Helpers;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities
{
    public class ValidateRuleFormTask : TaskActivity
    {
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ValidationRuleHelpers _validationRuleHelpers;
        private readonly IStringLocalizer S;

        public ValidateRuleFormTask(
            IHttpContextAccessor httpContextAccessor,
            IUpdateModelAccessor updateModelAccessor,
            IValidationRuleService validationRuleService,
            ValidationRuleHelpers validationRuleHelpers,
            IStringLocalizer<ValidateRuleFormTask> localizer
        )
        {
            _updateModelAccessor = updateModelAccessor;
            _httpContextAccessor = httpContextAccessor;
            _validationRuleHelpers = validationRuleHelpers;
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

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var form = _httpContextAccessor.HttpContext.Request.Form;
            var validationRuleService = _httpContextAccessor.HttpContext.RequestServices.GetService<IValidationRuleService>();
            var contentItemId = form["contentItemId"];
            if (String.IsNullOrEmpty(contentItemId)) throw new InvalidOperationException("Cannot find contentItem");
            var updater = _updateModelAccessor.ModelUpdater;
            if (updater == null) throw new InvalidOperationException("Cannot add model validation errors when there's no Updater present.");
            var flowParts = await _validationRuleHelpers.GeFlowPartFromContentItemId(contentItemId);

            foreach (var item in flowParts)
            {
                foreach (var formFlowWidget in item.Widgets)
                {
                    var validationRulePart = formFlowWidget.As<ValidationRulePart>();
                    if (validationRulePart == null) continue;
                    var inputPart = formFlowWidget.As<InputPart>();
                    var textAreaPart = formFlowWidget.As<TextAreaPart>();
                    var inputName = inputPart != null ? inputPart.ContentItem.DisplayText : textAreaPart != null ? textAreaPart.ContentItem.DisplayText : "";
                    if (String.IsNullOrEmpty(inputName)) continue;
                    var formInput = form[inputName];
                    var model = new ValidationRuleInput
                    {
                        Option = validationRulePart.Option,
                        Input = formInput,
                        Type = validationRulePart.Type,
                    };
                    var validationResult = await validationRuleService.ValidateInputByRuleAsync(model);
                    if (!validationResult) updater.ModelState.AddModelError(nameof(inputPart.ContentItem.DisplayText), S[validationRulePart.ErrorMessage]);
                }
            }

            var isValid = updater.ModelState.ErrorCount == 0;
           
            var outcome = isValid ? "Valid" : "Invalid";
            return Outcomes(outcome);
        }

    }
}
