using System.Security.Claims;

namespace OrchardCore.Security;

public static class StandardClaims
{
    /// <summary>
    /// This claim is assigned by the system during the login process if the user belongs to the Administrator role.
    /// </summary>
    public static readonly Claim SiteOwner = new("SiteOwner", "true");
}
