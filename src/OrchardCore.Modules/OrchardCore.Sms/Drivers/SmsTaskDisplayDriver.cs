using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Sms.Activities;
using OrchardCore.Sms.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Sms.Drivers;

public sealed class SmsTaskDisplayDriver : ActivityDisplayDriver<SmsTask, SmsTaskViewModel>
{
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    internal readonly IStringLocalizer S;

    public SmsTaskDisplayDriver(
        IPhoneFormatValidator phoneFormatValidator,
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<SmsTaskDisplayDriver> stringLocalizer
        )
    {
        _phoneFormatValidator = phoneFormatValidator;
        _liquidTemplateManager = liquidTemplateManager;
        S = stringLocalizer;
    }

    protected override void EditActivity(SmsTask activity, SmsTaskViewModel model)
    {
        model.PhoneNumber = activity.PhoneNumber.Expression;
        model.Body = activity.Body.Expression;
    }

    public override async Task<IDisplayResult> UpdateAsync(SmsTask activity, UpdateEditorContext context)
    {
        var viewModel = new SmsTaskViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        if (string.IsNullOrWhiteSpace(viewModel.PhoneNumber))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.PhoneNumber), S["Phone number requires a value."]);
        }
        else if (!_phoneFormatValidator.IsValid(viewModel.PhoneNumber))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.PhoneNumber), S["Invalid phone number used."]);
        }

        if (string.IsNullOrWhiteSpace(viewModel.Body))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Body), S["Message Body requires a value."]);
        }
        else if (!_liquidTemplateManager.Validate(viewModel.Body, out var bodyErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Body), string.Join(' ', bodyErrors));
        }

        activity.PhoneNumber = new WorkflowExpression<string>(viewModel.PhoneNumber);
        activity.Body = new WorkflowExpression<string>(viewModel.Body);

        return await EditAsync(activity, context);
    }
}
