using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public sealed class PermissionProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        Permissions.ManageMedia,
        Permissions.ManageMediaFolder,
        Permissions.ManageOthersMedia,
        Permissions.ManageOwnMedia,
        Permissions.ManageAttachedMediaFieldsFolder,
        Permissions.ManageMediaProfiles,
        Permissions.ViewMediaOptions,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        Permissions.ManageOwnMedia,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions =
            [
                Permissions.ManageMediaFolder,
                Permissions.ManageMediaProfiles,
                Permissions.ViewMediaOptions,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                Permissions.ManageMedia,
                Permissions.ManageOwnMedia,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Author,
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Contributor,
            Permissions = _generalPermissions,
        },
    ];
}
