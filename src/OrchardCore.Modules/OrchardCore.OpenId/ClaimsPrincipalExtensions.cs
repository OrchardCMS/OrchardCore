using System;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId
{
    internal static class ClaimsPrincipalExtensions
    {
        internal static string GetUserIdentifier(this ClaimsPrincipal principal)
            => principal.FindFirst(Claims.Subject)?.Value ??
               principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               principal.FindFirst(ClaimTypes.Upn)?.Value ??
               throw new InvalidOperationException("No suitable user identifier can be found in the principal.");
    }
}
