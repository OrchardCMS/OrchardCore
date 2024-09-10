using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenId;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageApplications
        = new("ManageApplications", "View, add, edit and remove the OpenID Connect applications.");

    public static readonly Permission ManageScopes
        = new("ManageScopes", "View, add, edit and remove the OpenID Connect scopes.");

    public static readonly Permission ManageClientSettings
        = new("ManageClientSettings", "View and edit the OpenID Connect client settings.");

    public static readonly Permission ManageServerSettings
        = new("ManageServerSettings", "View and edit the OpenID Connect server settings.");

    public static readonly Permission ManageValidationSettings
        = new("ManageValidationSettings", "View and edit the OpenID Connect validation settings.");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageApplications,
        ManageScopes,
        ManageClientSettings,
        ManageServerSettings,
        ManageValidationSettings,
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
