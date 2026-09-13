using OrchardCore.Security.Permissions;

namespace OrchardCore.HomeRoute;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission SetHomeRoute = new("SetHomeRoute", "Set the home route");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult<IEnumerable<Permission>>([SetHomeRoute]);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = [SetHomeRoute],
        },
    ];
}
