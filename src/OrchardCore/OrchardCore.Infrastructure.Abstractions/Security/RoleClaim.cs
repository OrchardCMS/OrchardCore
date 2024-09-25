using System.Security.Claims;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

/// <summary>
/// Represents a claim that is granted to all users within a role.
/// </summary>
public class RoleClaim
{
    /// <summary>
    /// Gets or sets the claim type for this claim.
    /// </summary>
    public string ClaimType { get; set; }

    /// <summary>
    /// Gets or sets the claim value for this claim.
    /// </summary>
    public string ClaimValue { get; set; }

    public RoleClaim()
    {
    }

    public RoleClaim(string value)
    {
        ClaimType = Permission.ClaimType;
        ClaimValue = value;
    }

    public RoleClaim(string value, string type)
    {
        ClaimType = type;
        ClaimValue = value;
    }

    public Claim ToClaim()
    {
        return new Claim(ClaimType, ClaimValue);
    }

    public RoleClaim Clone()
    {
        return new RoleClaim(value: ClaimValue, type: ClaimType);
    }

    public static implicit operator Claim(RoleClaim claim)
    {
        return claim.ToClaim();
    }
}
