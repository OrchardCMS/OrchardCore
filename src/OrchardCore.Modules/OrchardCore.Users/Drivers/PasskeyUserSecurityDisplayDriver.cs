using System.Buffers.Text;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

internal sealed class PasskeyUserSecurityDisplayDriver : DisplayDriver<UserSecurity>
{
    private readonly PasskeyService _passkeyService;
    private readonly ISiteService _siteService;
    private readonly TwoFactorOptions _twoFactorOptions;

    public PasskeyUserSecurityDisplayDriver(
        PasskeyService passkeyService,
        ISiteService siteService,
        IOptions<TwoFactorOptions> twoFactorOptions)
    {
        _passkeyService = passkeyService;
        _siteService = siteService;
        _twoFactorOptions = twoFactorOptions.Value;
    }

    public override IDisplayResult Display(UserSecurity model, BuildDisplayContext context)
    {
        if (model.User is not User user)
        {
            return null;
        }

        return Initialize<PasskeysViewModel>("UserSecurityPasskeys", async viewModel =>
        {
            var loginSettings = await _siteService.GetSettingsAsync<LoginSettings>();

            viewModel.IsLoginMethod = !loginSettings.DisableLocalLogin;
            viewModel.CanAddPasskey = await _passkeyService.CanAddPasskeyAsync(user);
            viewModel.IsTwoFactorMethod = _twoFactorOptions.Providers.Contains(UserConstants.PasskeyTwoFactorProvider);
            viewModel.Passkeys = user.Passkeys
                .OrderBy(passkey => passkey.CreatedUtc)
                .Select(passkey => new PasskeyEntryViewModel
                {
                    Id = Base64Url.EncodeToString(passkey.CredentialId),
                    Name = passkey.Name,
                    CreatedUtc = passkey.CreatedUtc,
                    LastUsedUtc = passkey.LastUsedUtc,
                    IsBackedUp = passkey.IsBackedUp,
                })
                .ToList();
        }).Location("Content:10");
    }
}
