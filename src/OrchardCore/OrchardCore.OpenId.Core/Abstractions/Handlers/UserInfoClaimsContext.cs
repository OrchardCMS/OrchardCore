using System.Collections.Generic;
using System.Security.Claims;

namespace OrchardCore.OpenId.Abstractions.Handlers;

public class UserInfoClaimsContext
{
    public UserInfoClaimsContext(ClaimsPrincipal principal, IDictionary<string, object> claims)
    {
        Principal = principal;
        Claims = claims;
    }

    /// <summary>
    /// Gets the <see cref="ClaimsPrincipal"/> representing the authenticated user.
    /// Implementations can read claims and scopes directly from this principal.
    /// </summary>
    public ClaimsPrincipal Principal { get; }

    public IDictionary<string, object> Claims { get; }
}
