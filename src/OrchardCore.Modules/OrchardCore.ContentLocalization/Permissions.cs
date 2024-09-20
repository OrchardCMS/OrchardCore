using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        ContentLocalizationPermissions.LocalizeContent,
        ContentLocalizationPermissions.LocalizeOwnContent,
        ContentLocalizationPermissions.ManageContentCulturePicker,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        ContentLocalizationPermissions.LocalizeOwnContent,
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
