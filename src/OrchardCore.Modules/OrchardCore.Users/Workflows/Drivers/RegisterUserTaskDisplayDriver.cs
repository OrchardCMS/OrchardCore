using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Users.Workflows.Drivers;

public sealed class RegisterUserTaskDisplayDriver : ActivityDisplayDriver<RegisterUserTask, RegisterUserTaskViewModel>
{
    internal readonly IStringLocalizer S;

    public RegisterUserTaskDisplayDriver(IStringLocalizer<RegisterUserTaskDisplayDriver> localizer)
    {
        S = localizer;
    }

    protected override void EditActivity(RegisterUserTask activity, RegisterUserTaskViewModel model)
    {
        model.SendConfirmationEmail = activity.SendConfirmationEmail;
        model.ConfirmationEmailSubject = activity.ConfirmationEmailSubject.Expression;
        model.ConfirmationEmailTemplate = activity.ConfirmationEmailTemplate.Expression;
        model.RequireModeration = activity.RequireModeration;
    }

    public override async Task<IDisplayResult> UpdateAsync(RegisterUserTask model, UpdateEditorContext context)
    {
        var viewModel = new RegisterUserTaskViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        model.SendConfirmationEmail = viewModel.SendConfirmationEmail;
        model.RequireModeration = viewModel.RequireModeration;
        model.ConfirmationEmailSubject = new WorkflowExpression<string>(viewModel.ConfirmationEmailSubject);
        model.ConfirmationEmailTemplate = new WorkflowExpression<string>(viewModel.ConfirmationEmailTemplate);

        if (model.SendConfirmationEmail)
        {
            if (string.IsNullOrWhiteSpace(viewModel.ConfirmationEmailSubject))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.ConfirmationEmailSubject), S["A value is required for {0}.", nameof(viewModel.ConfirmationEmailSubject)]);
            }

            if (string.IsNullOrWhiteSpace(viewModel.ConfirmationEmailTemplate))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.ConfirmationEmailTemplate), S["A value is required for {0}.", nameof(viewModel.ConfirmationEmailTemplate)]);
            }
        }

        return Edit(model, context);
    }
}
