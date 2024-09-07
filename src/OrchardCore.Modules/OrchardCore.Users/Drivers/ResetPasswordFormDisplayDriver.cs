using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class ResetPasswordFormDisplayDriver : DisplayDriver<ResetPasswordForm>
{
    public override IDisplayResult Edit(ResetPasswordForm model, BuildEditorContext context)
    {
        return Initialize<ResetPasswordViewModel>("ResetPasswordFormIdentifier", vm =>
        {
            vm.UsernameOrEmail = model.UsernameOrEmail;
            vm.NewPassword = model.NewPassword;
            vm.ResetToken = model.ResetToken;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ResetPasswordForm model, UpdateEditorContext context)
    {
        var vm = new ResetPasswordViewModel();

        await context.Updater.TryUpdateModelAsync(vm, Prefix);

        model.UsernameOrEmail = vm.UsernameOrEmail;
        model.NewPassword = vm.NewPassword;
        model.ResetToken = vm.ResetToken;

        return Edit(model, context);
    }
}

