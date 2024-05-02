using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class RegisterUserFormDisplayDriver : DisplayDriver<RegisterUserForm>
{
    private readonly UserManager<IUser> _userManager;
    private readonly IdentityOptions _identityOptions;

    private readonly IStringLocalizer S;

    public RegisterUserFormDisplayDriver(
        UserManager<IUser> userManager,
        IOptions<IdentityOptions> identityOptions,
        IStringLocalizer<RegisterUserFormDisplayDriver> stringLocalizer
        )
    {
        _userManager = userManager;
        _identityOptions = identityOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(RegisterUserForm model)
    {
        return Initialize<RegisterViewModel>("RegisterUserFormIdentifier", vm =>
        {
            vm.UserName = model.UserName;
            vm.Email = model.Email;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(RegisterUserForm model, IUpdateModel updater)
    {
        var vm = new RegisterViewModel();

        await updater.TryUpdateModelAsync(vm, Prefix);

        if (await _userManager.FindByNameAsync(vm.UserName) != null)
        {
            updater.ModelState.AddModelError(Prefix, nameof(vm.UserName), S["A user with the same username already exists."]);
        }
        else if (_identityOptions.User.RequireUniqueEmail && await _userManager.FindByEmailAsync(vm.Email) != null)
        {
            updater.ModelState.AddModelError(Prefix, nameof(vm.Email), S["A user with the same email address already exists."]);
        }

        model.UserName = vm.UserName;
        model.Email = vm.Email;
        model.Password = vm.Password;

        return Edit(model);
    }
}
