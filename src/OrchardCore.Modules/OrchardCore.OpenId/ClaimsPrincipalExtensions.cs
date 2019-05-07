using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using AspNet.Security.OpenIdConnect.Primitives;

namespace OrchardCore.OpenId
{
    internal static class ClaimsPrincipalExtensions
    {
        internal static string GetUserIdentifier(this ClaimsPrincipal principal)
            => principal.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value ??
               principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               principal.FindFirst(ClaimTypes.Upn)?.Value ??
               throw new InvalidOperationException("No suitable user identifier can be found in the principal.");
    }
}
