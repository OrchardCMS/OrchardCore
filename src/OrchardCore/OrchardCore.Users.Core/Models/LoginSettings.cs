using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Models
{
    public class LoginSettings
    {
        public bool UseSiteTheme { get; set; }

        public bool UseExternalProviderIfOnlyOneDefined { get; set; }

        public bool DisableLocalLogin { get; set; }

        public bool UseScriptToSyncRoles { get; set; }

        public string SyncRolesScript { get; set; }

        public bool AllowChangingUsername { get; set; }

        public bool AllowChangingEmail { get; set; }

        public bool EnableTwoFactorAuthentication { get; set; }

        public bool EnableTwoFactorAuthenticationForSpecificRoles { get; set; }

        public string[] Roles { get; set; } = Array.Empty<string>();

        public bool RequireTwoFactorAuthentication { get; set; }

        public bool AllowRememberClientTwoFactorAuthentication { get; set; }

        public bool UseEmailAsAuthenticatorDisplayName { get; set; }

        public int NumberOfRecoveryCodesToGenerate { get; set; } = 10;

        public int TokenLength { get; set; } = 6;
    }

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
