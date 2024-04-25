using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class ForgotPasswordFormDisplayDriver : DisplayDriver<ForgotPasswordForm>
{
    public override IDisplayResult Edit(ForgotPasswordForm model)
    {
        return Initialize<ForgotPasswordViewModel>("ForgotPasswordFormIdentifier", vm =>
        {
            vm.UsernameOrEmail = model.UsernameOrEmail;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ForgotPasswordForm model, IUpdateModel updater)
    {
        var viewModel = new ForgotPasswordViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        model.UsernameOrEmail = viewModel.UsernameOrEmail;

        return Edit(model);
    }
}

