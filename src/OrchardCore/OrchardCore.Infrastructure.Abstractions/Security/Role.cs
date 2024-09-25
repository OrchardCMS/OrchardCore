using OrchardCore.Infrastructure.Security;

namespace OrchardCore.Security;

public class Role : IRole
{
    public string RoleName { get; set; }

    public string RoleDescription { get; set; }

    public string NormalizedRoleName { get; set; }

    public List<RoleClaim> RoleClaims { get; set; } = [];

    public RoleType Type { get; set; }

    public Role Clone()
    {
        var role = new Role
        {
            RoleName = RoleName,
            RoleDescription = RoleDescription,
            NormalizedRoleName = NormalizedRoleName,
            Type = Type,
        };

        foreach (var claim in RoleClaims)
        {
            role.RoleClaims.Add(claim.Clone());
        }

        return role;
    }
}
