using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public sealed class PermissionProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        MediaPermissions.AccessMediaApi,
        MediaPermissions.ManageMedia,
        MediaPermissions.ManageMediaFolder,
        MediaPermissions.ManageOthersMedia,
        MediaPermissions.ManageOwnMedia,
        MediaPermissions.ManageAttachedMediaFieldsFolder,
        MediaPermissions.ManageMediaProfiles,
        MediaPermissions.ViewMediaOptions,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        MediaPermissions.ManageOwnMedia,
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
                MediaPermissions.AccessMediaApi,
                MediaPermissions.ManageMediaFolder,
                MediaPermissions.ManageMediaProfiles,
                MediaPermissions.ViewMediaOptions,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                MediaPermissions.ManageMedia,
                MediaPermissions.ManageOwnMedia,
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
