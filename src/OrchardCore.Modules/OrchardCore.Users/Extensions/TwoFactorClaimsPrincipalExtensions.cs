using System.Security.Claims;

namespace OrchardCore.Users;

internal static class TwoFactorClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets whether the user has to set up two-factor authentication before proceeding.
    /// A user who logged in with multiple factors, for instance with a passkey, already meets the requirement.
    /// </summary>
    public static bool IsTwoFactorSetupPending(this ClaimsPrincipal principal)
        => principal.HasClaim(claim => claim.Type == UserConstants.TwoFactorAuthenticationClaimType) &&
            !principal.HasClaim(UserConstants.AuthenticationMethodsClaimType, UserConstants.MultiFactorAuthenticationMethod);
}
