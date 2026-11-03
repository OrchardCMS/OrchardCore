using System.Buffers.Text;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class PasskeyUserSecurityDisplayDriver : DisplayDriver<UserSecurity>
{
    private readonly TwoFactorOptions _twoFactorOptions;

    public PasskeyUserSecurityDisplayDriver(IOptions<TwoFactorOptions> twoFactorOptions)
    {
        _twoFactorOptions = twoFactorOptions.Value;
    }

    public override IDisplayResult Display(UserSecurity model, BuildDisplayContext context)
    {
        if (model.User is not User user)
        {
            return null;
        }

        return Initialize<PasskeysViewModel>("UserSecurityPasskeys", viewModel =>
        {
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
