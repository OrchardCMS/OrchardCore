using OrchardCore.Users.Localization.Providers;

namespace System.Security.Claims;

public static class ClaimsPrincipleExtensions
{
    public static string GetCulture(this ClaimsPrincipal principal)
        => principal.FindFirstValue(UserLocalizationClaimsProvider.CultureClaimType);
}
