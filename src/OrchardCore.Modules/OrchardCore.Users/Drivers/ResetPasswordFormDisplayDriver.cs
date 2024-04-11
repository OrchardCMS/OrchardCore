using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public class ResetPasswordFormDisplayDriver : DisplayDriver<ResetPasswordForm>
{
    public override IDisplayResult Edit(ResetPasswordForm model)
    {
        return Initialize<ResetPasswordViewModel>("ResetPasswordFormIdentifier_Edit", vm =>
        {
            vm.Identifier = model.Identifier;
            vm.NewPassword = model.NewPassword;
            vm.ResetToken = model.ResetToken;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ResetPasswordForm model, IUpdateModel updater)
    {
        var vm = new ResetPasswordViewModel();

        await updater.TryUpdateModelAsync(vm, Prefix);

        model.Identifier = vm.Identifier;
        model.NewPassword = vm.NewPassword;
        model.ResetToken = vm.ResetToken;

        return Edit(model);
    }
}

