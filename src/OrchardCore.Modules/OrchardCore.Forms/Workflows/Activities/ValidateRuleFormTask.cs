
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Scripting;
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
        private readonly IScriptingManager _scriptingManager;

        public ValidateRuleFormTask(
            IScriptingManager scriptingManager,
            IHttpContextAccessor httpContextAccessor,
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<ValidateRuleFormTask> localizer
        )
        {
            _scriptingManager = scriptingManager;
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
            bool validateRule = true;
            var rules = new List<dynamic>();
            foreach (var item in httpContext.Request.Form)
            {
                if (item.Key.Equals("validateRules", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(item.Value[0]))
                    {
                        rules = JsonConvert.DeserializeObject<List<dynamic>>(item.Value[0]);
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
                    var type = item.type.ToString();
                    var option = item.option.ToString();
                    var formItemValue = httpContext.Request.Form[item.elementId.ToString()];
                    var engine = _scriptingManager.GetScriptingEngine("js");
                    var scope = engine.CreateScope(_scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()), ShellScope.Services, null, null);
                    var jsScriptStr = $"js: {type}('{formItemValue}','{option}')";
                    if (!Convert.ToBoolean(engine.Evaluate(scope, jsScriptStr)))
                    {
                        validateRule = false;
                        break;
                    }
                }
            }

            var isValid = updater.ModelState.ErrorCount == 0;
            var outcome = isValid && validateRule ? "Valid" : "Invalid";
            return Outcomes(outcome);
        }
    }
}
