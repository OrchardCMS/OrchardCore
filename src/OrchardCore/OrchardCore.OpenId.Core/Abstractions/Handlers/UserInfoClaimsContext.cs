using System.Collections.Generic;
using System.Security.Claims;

namespace OrchardCore.OpenId.Abstractions.Handlers;

public class UserInfoClaimsContext
{
    public UserInfoClaimsContext(ClaimsIdentity identity, IDictionary<string, object> claims)
    {
        Identity = identity;
        Claims = claims;
    }

    /// <summary>
    /// Gets the <see cref="ClaimsIdentity"/> from the authenticated principal.
    /// Implementations can read claims directly from this identity.
    /// </summary>
    public ClaimsIdentity Identity { get; }

    public IDictionary<string, object> Claims { get; }
}
