using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public class ForgotPasswordFormDisplayDriver : DisplayDriver<ForgotPasswordForm>
{
    public override IDisplayResult Edit(ForgotPasswordForm model)
    {
        return Initialize<ForgotPasswordViewModel>("ForgotPasswordFormIdentifier_Edit", vm =>
        {
            vm.Identifier = model.Identifier;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ForgotPasswordForm model, IUpdateModel updater)
    {
        var viewModel = new ForgotPasswordViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        model.Identifier = viewModel.Identifier;

        return Edit(model);
    }
}

