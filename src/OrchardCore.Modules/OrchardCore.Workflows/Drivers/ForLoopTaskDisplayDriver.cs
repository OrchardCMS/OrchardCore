using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class ForLoopTaskDisplayDriver : ActivityDisplayDriver<ForLoopTask, ForLoopTaskViewModel>
{
    private readonly ILiquidTemplateManager _templateManager;
    private readonly IStringLocalizer S;

    public ForLoopTaskDisplayDriver(
        ILiquidTemplateManager templateManager,
        IStringLocalizer<ForLoopTaskDisplayDriver> stringLocalizer)
    {
        _templateManager = templateManager;
        S = stringLocalizer;
    }

    protected override void EditActivity(ForLoopTask activity, ForLoopTaskViewModel model)
    {
        model.FromExpression = activity.From.Expression;
        model.LiquidFromExpression = activity.LiquidFrom.Expression;
        model.ToExpression = activity.To.Expression;
        model.LiquidToExpression = activity.LiquidTo.Expression;
        model.LoopVariableName = activity.LoopVariableName;
        model.StepExpression = activity.Step.Expression;
        model.LiquidStepExpression = activity.LiquidStep.Expression;
        model.Syntax = activity.Syntax;
    }

    public override async Task<IDisplayResult> UpdateAsync(ForLoopTask activity, UpdateEditorContext context)
    {
        var model = new ForLoopTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.From = new WorkflowExpression<double>(model.FromExpression);
        activity.LiquidFrom = new WorkflowExpression<string>(model.LiquidFromExpression);
        activity.To = new WorkflowExpression<double>(model.ToExpression);
        activity.LiquidTo = new WorkflowExpression<string>(model.LiquidToExpression);
        activity.Step = new WorkflowExpression<double>(model.StepExpression);
        activity.LiquidStep = new WorkflowExpression<string>(model.LiquidStepExpression);
        activity.LoopVariableName = model.LoopVariableName?.Trim();
        activity.Syntax = model.Syntax;

        if (model.Syntax == WorkflowScriptSyntax.Liquid)
        {
            ValidateLiquidExpression(context, model.LiquidFromExpression, nameof(model.LiquidFromExpression), "From");
            ValidateLiquidExpression(context, model.LiquidToExpression, nameof(model.LiquidToExpression), "To");
            ValidateLiquidExpression(context, model.LiquidStepExpression, nameof(model.LiquidStepExpression), "Step");
        }
        else if (model.Syntax == WorkflowScriptSyntax.JavaScript)
        {
            ValidateRequired(context, model.FromExpression, nameof(model.FromExpression), "From");
            ValidateRequired(context, model.ToExpression, nameof(model.ToExpression), "To");
            ValidateRequired(context, model.StepExpression, nameof(model.StepExpression), "Step");
        }

        return Edit(activity, context);
    }

    private void ValidateLiquidExpression(UpdateEditorContext context, string expression, string propertyName, string label)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            context.Updater.ModelState.AddModelError(Prefix, propertyName, S["{0} is required field.", label]);
        }
        else if (!_templateManager.Validate(expression, out var errors))
        {
            context.Updater.ModelState.AddModelError(Prefix, propertyName, S["{0} doesn't contain a valid Liquid expression. Details: {1}", label, string.Join(" ", errors)]);
        }
    }

    private void ValidateRequired(UpdateEditorContext context, string expression, string propertyName, string label)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            context.Updater.ModelState.AddModelError(Prefix, propertyName, S["{0} is required field.", label]);
        }
    }
}
