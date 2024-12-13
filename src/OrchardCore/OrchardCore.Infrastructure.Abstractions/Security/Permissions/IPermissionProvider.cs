namespace OrchardCore.Security.Permissions;

/// <summary>
/// Implemented by modules to enumerate the types of permissions
/// the which may be granted.
/// </summary>
public interface IPermissionProvider
{
    Task<IEnumerable<Permission>> GetPermissionsAsync();
    IEnumerable<PermissionStereotype> GetDefaultStereotypes();
}
