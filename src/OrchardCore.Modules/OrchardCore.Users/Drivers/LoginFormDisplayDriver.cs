using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class LoginFormDisplayDriver : DisplayDriver<LoginForm>
{
    public override IDisplayResult Edit(LoginForm model)
    {
        return Initialize<LoginViewModel>("LoginFormCredentials", vm =>
        {
            vm.UserName = model.UserName;
            vm.RememberMe = model.RememberMe;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(LoginForm model, IUpdateModel updater)
    {
        var viewModel = new LoginViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        model.UserName = viewModel.UserName;
        model.Password = viewModel.Password;
        model.RememberMe = viewModel.RememberMe;

        return Edit(model);
    }
}
