using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class TwoFactorUserSecurityDisplayDriver : DisplayDriver<UserSecurity>
{
    private readonly UserManager<IUser> _userManager;
    private readonly SignInManager<IUser> _signInManager;
    private readonly ITwoFactorAuthenticationHandlerCoordinator _twoFactorHandlerCoordinator;
    private readonly IDisplayManager<TwoFactorMethod> _twoFactorMethodDisplayManager;
    private readonly TwoFactorOptions _twoFactorOptions;

    public TwoFactorUserSecurityDisplayDriver(
        UserManager<IUser> userManager,
        SignInManager<IUser> signInManager,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorHandlerCoordinator,
        IDisplayManager<TwoFactorMethod> twoFactorMethodDisplayManager,
        IOptions<TwoFactorOptions> twoFactorOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _twoFactorHandlerCoordinator = twoFactorHandlerCoordinator;
        _twoFactorMethodDisplayManager = twoFactorMethodDisplayManager;
        _twoFactorOptions = twoFactorOptions.Value;
    }

    public override IDisplayResult Display(UserSecurity model, BuildDisplayContext context)
    {
        return Initialize<TwoFactorAuthenticationViewModel>("UserSecurityTwoFactor", async viewModel =>
        {
            var user = model.User;
            var providers = (await _userManager.GetValidTwoFactorProvidersAsync(user))
                .Intersect(_twoFactorOptions.Providers)
                .ToList();

            viewModel.User = user;
            viewModel.IsTwoFaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            viewModel.IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            viewModel.RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);
            viewModel.CanDisableTwoFactor = !await _twoFactorHandlerCoordinator.IsRequiredAsync(user);
            viewModel.ValidTwoFactorProviders = providers.Select(provider => new SelectListItem(provider, provider)).ToList();

            if (user is User u && u.TryGet<TwoFactorPreference>(out var preference))
            {
                viewModel.PreferredProvider = preference.DefaultProvider;
            }

            foreach (var provider in _twoFactorOptions.Providers)
            {
                var method = new TwoFactorMethod
                {
                    Provider = provider,
                    IsEnabled = providers.Contains(provider),
                };

                viewModel.AuthenticationMethods.Add(await _twoFactorMethodDisplayManager.BuildDisplayAsync(method, context.Updater, OrchardCoreConstants.DisplayType.SummaryAdmin));
            }
        }).Location("Content:5");
    }
}
