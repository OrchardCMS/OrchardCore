using System;
using System.Security.Claims;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Users.Localization.Models;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Localization.Providers;

public class UserLocalizationClaimsProvider : IUserClaimsProvider
{
    public Task GenerateAsync(IUser user, ClaimsIdentity claims)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(claims);

        if (user is not User u)
        {
            return Task.CompletedTask;
        }

        if (u.Has<UserLocalizationSettings>())
        {
            var localizationSetting = u.As<UserLocalizationSettings>();

            if (!string.IsNullOrEmpty(localizationSetting.Culture))
            {
                claims.AddClaim(new Claim("culture", localizationSetting.Culture));
            }
        }

        return Task.CompletedTask;
    }
}
