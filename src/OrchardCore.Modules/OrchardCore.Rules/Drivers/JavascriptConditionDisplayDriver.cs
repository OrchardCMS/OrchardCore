using System;
using System.Threading.Tasks;
using Esprima;
using Jint.Runtime;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers
{
    public class JavascriptConditionDisplayDriver : DisplayDriver<Condition, JavascriptCondition>
    {
        private readonly ConditionOperatorOptions _options;
        private readonly IHtmlLocalizer H;
        private readonly IStringLocalizer S;
        private readonly INotifier _notifier;
        private readonly JavascriptConditionEvaluator _evaluator;

        public JavascriptConditionDisplayDriver(
            IOptions<ConditionOperatorOptions> options,
            IHtmlLocalizer<JavascriptConditionDisplayDriver> htmlLocalizer,
            IStringLocalizer<JavascriptConditionDisplayDriver> stringLocalizer,
            JavascriptConditionEvaluator evaluator,
            INotifier notifier)
        {
            _options = options.Value;
            H = htmlLocalizer;
            S = stringLocalizer;
            _evaluator = evaluator;
            _notifier = notifier;
        }

        public override IDisplayResult Display(JavascriptCondition condition)
        {
            return
                Combine(
                    View("JavascriptCondition_Fields_Summary", condition).Location("Summary", "Content"),
                    View("JavascriptCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(JavascriptCondition condition)
        {
            return Initialize<JavascriptConditionViewModel>("JavascriptCondition_Fields_Edit", m =>
            {
                m.Script = condition.Script;
                m.Condition = condition;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(JavascriptCondition condition, IUpdateModel updater)
        {
            var model = new JavascriptConditionViewModel();
            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                if (string.IsNullOrWhiteSpace(model.Script))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(model.Script), S["The script is required"]);
                    //Codemirror hides the textarea which displays the error when updater.ModelState.AddModelError is used, that's why a notifier is used to show the error to user
                    await _notifier.ErrorAsync(H["The script is required"]);
                    return Edit(condition);
                }

                try
                {
                    _ = await _evaluator.EvaluateAsync(new()
                    {
                        ConditionId = condition.ConditionId,
                        Name = condition.Name,
                        Script = model.Script
                    });
                    condition.Script = model.Script;
                }
                catch (ParserException ex) //Invalid syntax
                {
                    updater.ModelState.AddModelError(Prefix, nameof(model.Script), S["The script couldn't be parsed. Details: {0}", ex.Message]);
                    await _notifier.ErrorAsync(H["The script couldn't be parsed. Details: {0}", ex.Message]);
                }
                catch (JavaScriptException ex) //Evaluation threw an Error
                {
                    updater.ModelState.AddModelError(Prefix, nameof(model.Script), S["JavaScript evaluation resulted in an exception. Details: {0}", ex.Message]);
                    await _notifier.ErrorAsync(H["JavaScript evaluation resulted in an exception. Details: {0}", ex.Message]);
                }
                catch (Exception ex) when (ex is InvalidCastException or FormatException) //Evaluation completes successfully, but the result cannot be converted to Boolean
                {
                    updater.ModelState.AddModelError(Prefix, nameof(model.Script), S["The script evaluation failed. Details: {0}", ex.Message]);
                    await _notifier.ErrorAsync(H["The script evaluation failed. Details: {0}", ex.Message]);
                }
            }

            return Edit(condition);
        }
    }
}
