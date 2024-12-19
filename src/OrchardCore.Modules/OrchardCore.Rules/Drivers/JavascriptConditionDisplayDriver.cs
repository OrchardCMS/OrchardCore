using Acornima;
using Jint.Runtime;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers;

public sealed class JavascriptConditionDisplayDriver : DisplayDriver<Condition, JavascriptCondition>
{
    private readonly INotifier _notifier;
    private readonly JavascriptConditionEvaluator _evaluator;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public JavascriptConditionDisplayDriver(
        IHtmlLocalizer<JavascriptConditionDisplayDriver> htmlLocalizer,
        IStringLocalizer<JavascriptConditionDisplayDriver> stringLocalizer,
        JavascriptConditionEvaluator evaluator,
        INotifier notifier)
    {
        H = htmlLocalizer;
        S = stringLocalizer;
        _evaluator = evaluator;
        _notifier = notifier;
    }

    public override Task<IDisplayResult> DisplayAsync(JavascriptCondition condition, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("JavascriptCondition_Fields_Summary", condition).Location("Summary", "Content"),
                View("JavascriptCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(JavascriptCondition condition, BuildEditorContext context)
    {
        return Initialize<JavascriptConditionViewModel>("JavascriptCondition_Fields_Edit", m =>
        {
            m.Script = condition.Script;
            m.Condition = condition;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(JavascriptCondition condition, UpdateEditorContext context)
    {
        var model = new JavascriptConditionViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        // CodeMirror hides the textarea which displays the error when updater.ModelState.AddModelError() is used,
        // that's why a notifier is used to show validation errors.
        if (string.IsNullOrWhiteSpace(model.Script))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Script), S["Please provide a script."]);
            await _notifier.ErrorAsync(H["Please provide a script."]);

            return Edit(condition, context);
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
        catch (ParseErrorException ex) // Invalid syntax
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Script), S["The script couldn't be parsed. Details: {0}", ex.Message]);
            await _notifier.ErrorAsync(H["The script couldn't be parsed. Details: {0}", ex.Message]);
        }
        catch (JavaScriptException ex) // Evaluation threw an Error
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Script), S["JavaScript evaluation resulted in an exception. Details: {0}", ex.Message]);
            await _notifier.ErrorAsync(H["JavaScript evaluation resulted in an exception. Details: {0}", ex.Message]);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException) // Evaluation completes successfully, but the result cannot be converted to Boolean
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Script), S["The script evaluation failed. Details: {0}", ex.Message]);
            await _notifier.ErrorAsync(H["The script evaluation failed. Details: {0}", ex.Message]);
        }

        return Edit(condition, context);
    }
}
