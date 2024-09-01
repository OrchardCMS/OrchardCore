namespace OrchardCore.Security.Permissions;

public class PermissionStereotype
{
    public string Name { get; set; }

    public IEnumerable<Permission> Permissions { get; set; }
}
