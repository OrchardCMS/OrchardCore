namespace OrchardCore.Security;

public class Role : IRole
{
    public string RoleName { get; set; }

    public string RoleDescription { get; set; }

    public string NormalizedRoleName { get; set; }

    public List<RoleClaim> RoleClaims { get; set; } = [];

    public Role Clone()
    {
        var role = new Role
        {
            RoleName = RoleName,
            RoleDescription = RoleDescription,
            NormalizedRoleName = NormalizedRoleName,
        };

        foreach (var claim in RoleClaims)
        {
            role.RoleClaims.Add(claim.Clone());
        }

        return role;
    }
}
