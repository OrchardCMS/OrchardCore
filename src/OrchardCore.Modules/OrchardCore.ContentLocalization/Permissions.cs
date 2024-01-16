using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization;

public class Permissions : IPermissionProvider
{
    public static readonly Permission LocalizeContent = new("LocalizeContent", "Localize content for others");
    public static readonly Permission LocalizeOwnContent = new("LocalizeOwnContent", "Localize own content", new[] { LocalizeContent });
    public static readonly Permission ManageContentCulturePicker = new("ManageContentCulturePicker", "Manage ContentCulturePicker settings");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        LocalizeContent,
        LocalizeOwnContent,
        ManageContentCulturePicker,
    ];

    private static readonly IEnumerable<Permission> _generalPermissions =
    [
        LocalizeOwnContent,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Author",
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = "Contributor",
            Permissions = _generalPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
