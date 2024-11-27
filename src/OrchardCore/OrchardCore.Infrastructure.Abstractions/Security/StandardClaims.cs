using System.Security.Claims;

namespace OrchardCore.Security;

public static class StandardClaims
{
    /// <summary>
    /// This claim is assigned by the system during the login process if the user belongs to the Administrator role.
    /// </summary>
    [Obsolete("This claim is obsolete and will be removed in the next major version.")]
    public static readonly Claim SiteOwner = new("SiteOwner", "true");
}
