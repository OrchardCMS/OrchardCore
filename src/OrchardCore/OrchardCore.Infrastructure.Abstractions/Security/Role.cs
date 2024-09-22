using OrchardCore.Infrastructure.Security;

namespace OrchardCore.Security;

public class Role : IRole
{
    public string RoleName { get; set; }

    public string RoleDescription { get; set; }

    public string NormalizedRoleName { get; set; }

    public List<RoleClaim> RoleClaims { get; set; } = [];

    public bool HasFullAccess { get; set; }

    public RoleType Type { get; set; }

    public Role Clone()
    {
        var role = new Role
        {
            RoleName = RoleName,
            RoleDescription = RoleDescription,
            NormalizedRoleName = NormalizedRoleName,
            HasFullAccess = HasFullAccess,
            Type = Type,
        };

        foreach (var claim in RoleClaims)
        {
            role.RoleClaims.Add(new RoleClaim
            {
                ClaimType = claim.ClaimType,
                ClaimValue = claim.ClaimValue,
            });
        }

        return role;
    }
}
