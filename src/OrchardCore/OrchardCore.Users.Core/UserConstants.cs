namespace OrchardCore.Users;

public static class UserConstants
{
    public const string EmailConfirmationAuthenticationScheme = "Identity.EmailConfirmation";

    public const string TwoFactorAuthenticationClaimType = "TwoFacAuth";

    /// <summary>
    /// The authentication scheme of the short-lived cookie that holds the state of a passkey registration or sign-in
    /// between the moment the browser receives the options and the moment it returns the credential.
    /// </summary>
    public const string PasskeyAuthenticationScheme = "Identity.Passkey";

    /// <summary>
    /// The name of the two-factor provider that verifies the second factor with a passkey.
    /// </summary>
    public const string PasskeyTwoFactorProvider = "Passkey";

    /// <summary>
    /// The type of the claim that lists the methods used to authenticate the user, as defined by RFC 8176.
    /// </summary>
    public const string AuthenticationMethodsClaimType = "amr";

    /// <summary>
    /// The authentication method of a user who proved the possession of a key, such as a passkey.
    /// </summary>
    public const string ProofOfPossessionAuthenticationMethod = "pop";

    /// <summary>
    /// The authentication method of a user who authenticated with multiple factors.
    /// </summary>
    public const string MultiFactorAuthenticationMethod = "mfa";

    public static class Features
    {
        public const string Users = "OrchardCore.Users";

        public const string TwoFactorAuthentication = "OrchardCore.Users.2FA";

        public const string AuthenticatorApp = "OrchardCore.Users.2FA.AuthenticatorApp";

        public const string EmailAuthenticator = "OrchardCore.Users.2FA.Email";

        public const string SmsAuthenticator = "OrchardCore.Users.2FA.Sms";

        public const string UserRegistration = "OrchardCore.Users.Registration";

        public const string ExternalAuthentication = "OrchardCore.Users.ExternalAuthentication";

        public const string ResetPassword = "OrchardCore.Users.ResetPassword";

        public const string Passkeys = "OrchardCore.Users.Passkeys";

        public const string UserSecurity = "OrchardCore.Users.Security";
    }
}
