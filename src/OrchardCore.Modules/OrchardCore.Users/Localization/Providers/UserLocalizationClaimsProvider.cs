using System.Security.Claims;
using OrchardCore.Entities;
using OrchardCore.Users.Localization.Models;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Localization.Providers;

public class UserLocalizationClaimsProvider : IUserClaimsProvider
{
    internal const string CultureClaimType = "culture";

    public Task GenerateAsync(IUser user, ClaimsIdentity claims)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(claims);

        if (user is not User currentUser)
        {
            return Task.CompletedTask;
        }

        if (currentUser.TryGet<UserLocalizationSettings>(out var localizationSetting) && localizationSetting.Culture != "none")
        {
            claims.AddClaim(new Claim(CultureClaimType, localizationSetting.Culture == UserLocalizationConstants.Invariant ? "" : localizationSetting.Culture));
        }

        return Task.CompletedTask;
    }
}
