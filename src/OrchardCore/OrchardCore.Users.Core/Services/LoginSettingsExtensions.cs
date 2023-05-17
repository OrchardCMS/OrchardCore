using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Models
{
    public static class LoginSettingsExtensions
    {
        public static bool IsTwoFactorAuthenticationEnabled(this LoginSettings settings)
        {
            return settings.EnableTwoFactorAuthentication
                || settings.EnableTwoFactorAuthenticationForSpecificRoles;
        }

        public static async Task<bool> CanEnableTwoFactorAuthenticationAsync(this LoginSettings loginSettings, Func<string, Task<bool>> isInRole)
        {
            if (loginSettings.EnableTwoFactorAuthenticationForSpecificRoles)
            {
                foreach (var role in loginSettings.Roles)
                {
                    if (await isInRole(role))
                    {
                        return true;
                    }
                }

                return false;
            }

            return loginSettings.EnableTwoFactorAuthentication;
        }
    }
}
