namespace OrchardCore.Users;

public static class UserRateLimiterPolicyNames
{
    public const string PasswordAuthentication = "password-authentication";

    public const string PasswordRecovery = "password-recovery";

    public const string UserRegistration = "user-registration";

    public const string TwoFactorAuthentication = "two-factor-authentication";

    public const string TwoFactorRecovery = "two-factor-recovery";

    public const string TwoFactorCodeSend = "two-factor-code-send";
}
