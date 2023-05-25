using System;

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

        public int NumberOfRecoveryCodesToGenerate { get; set; } = 5;

        public int TokenLength { get; set; } = 6;
    }
}
