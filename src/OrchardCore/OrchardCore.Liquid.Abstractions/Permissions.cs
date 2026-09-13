using OrchardCore.Security.Permissions;

namespace OrchardCore.Liquid;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageLiquidTemplates = new(
        "ManageLiquidTemplates",
        "Manage Liquid templates",
        isSecurityCritical: true);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageLiquidTemplates,
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
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _allPermissions,
        },
    ];
}
