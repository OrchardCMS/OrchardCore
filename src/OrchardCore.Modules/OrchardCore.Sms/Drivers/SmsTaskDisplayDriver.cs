using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Sms.Activities;
using OrchardCore.Sms.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Sms.Drivers;

public class SmsTaskDisplayDriver : ActivityDisplayDriver<SmsTask, SmsTaskViewModel>
{
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    protected readonly IStringLocalizer S;

    public SmsTaskDisplayDriver(
        IPhoneFormatValidator phoneFormatValidator,
        IStringLocalizer<SmsTaskDisplayDriver> stringLocalizer,
        ILiquidTemplateManager liquidTemplateManager)
    {
        _phoneFormatValidator = phoneFormatValidator;
        S = stringLocalizer;
        _liquidTemplateManager = liquidTemplateManager;
    }

    protected override void EditActivity(SmsTask activity, SmsTaskViewModel model)
    {
        model.PhoneNumber = activity.PhoneNumber.Expression;
        model.Body = activity.Body.Expression;
    }

    public async override Task<IDisplayResult> UpdateAsync(SmsTask activity, IUpdateModel updater)
    {
        var viewModel = new SmsTaskViewModel();

        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            if (String.IsNullOrWhiteSpace(viewModel.PhoneNumber))
            {
                updater.ModelState.AddModelError(Prefix, nameof(viewModel.PhoneNumber), S["Phone number requires a value."]);
            }
            else if (!_phoneFormatValidator.IsValid(viewModel.PhoneNumber))
            {
                updater.ModelState.AddModelError(Prefix, nameof(viewModel.PhoneNumber), S["Invalid phone number used."]);
            }

            if (String.IsNullOrWhiteSpace(viewModel.Body))
            {
                updater.ModelState.AddModelError(Prefix, nameof(viewModel.Body), S["Message Body requires a value."]);
            }
            else if (!_liquidTemplateManager.Validate(viewModel.Body, out var bodyErrors))
            {
                updater.ModelState.AddModelError(Prefix, nameof(viewModel.Body), String.Join(' ', bodyErrors));
            }

            activity.PhoneNumber = new WorkflowExpression<string>(viewModel.PhoneNumber);
            activity.Body = new WorkflowExpression<string>(viewModel.Body);
        }

        return Edit(activity);
    }
}
