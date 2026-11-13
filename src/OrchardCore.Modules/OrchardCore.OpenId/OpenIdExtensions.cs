using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId;

internal static class OpenIdExtensions
{
    internal static string GetUserIdentifier(this ClaimsIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        return identity.FindFirst(Claims.Subject)?.Value ??
           identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
           identity.FindFirst(ClaimTypes.Upn)?.Value ??
           throw new InvalidOperationException("No suitable user identifier can be found in the identity.");
    }

    internal static string GetUserIdentifier(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindUserIdentifier() ??
           throw new InvalidOperationException("No suitable user identifier can be found in the principal.");
    }

    internal static string FindUserIdentifier(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindFirst(Claims.Subject)?.Value ??
           principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
           principal.FindFirst(ClaimTypes.Upn)?.Value;
    }

    internal static string GetUserName(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindFirst(Claims.Name)?.Value ??
           principal.FindFirst(ClaimTypes.Name)?.Value ??
           throw new InvalidOperationException("No suitable user name can be found in the principal.");
    }

    internal static string[] GetRoles(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindAll(c => c.Type is Claims.Role or ClaimTypes.Role)
        .Select(x => x.Value)
        .Distinct()
        .ToArray();
    }
}
