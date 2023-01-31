using System.Security.Claims;
using OrchardCore.Users.Localization.Models;
using OrchardCore.Entities;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using System.Threading.Tasks;
using System;

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

            if (!String.IsNullOrEmpty(localizationSetting.Culture))
            {
                claims.AddClaim(new Claim("culture", localizationSetting.Culture));
            }
        }

        return Task.CompletedTask;
    }
}
