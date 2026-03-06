namespace OrchardCore.Security;

public class Role : IRole
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string NormalizedRoleName { get; set; }

    public List<RoleClaim> RoleClaims { get; set; } = [];

    public Role Clone()
    {
        var role = new Role
        {
            Name = Name,
            Description = Description,
            NormalizedRoleName = NormalizedRoleName,
        };

        foreach (var claim in RoleClaims)
        {
            role.RoleClaims.Add(claim.Clone());
        }

        return role;
    }
}
