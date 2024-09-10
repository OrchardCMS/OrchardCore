using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageBackgroundTasks = new("ManageBackgroundTasks", "Manage background tasks");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageBackgroundTasks,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
