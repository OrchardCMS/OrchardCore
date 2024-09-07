using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission LocalizeContent = new("LocalizeContent", "Localize content for others");
    public static readonly Permission LocalizeOwnContent = new("LocalizeOwnContent", "Localize own content", new[] { LocalizeContent });
    public static readonly Permission ManageContentCulturePicker = new("ManageContentCulturePicker", "Manage ContentCulturePicker settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        LocalizeContent,
        LocalizeOwnContent,
        ManageContentCulturePicker,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        LocalizeOwnContent,
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
