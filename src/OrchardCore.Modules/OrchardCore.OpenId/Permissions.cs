using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenId;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        OpenIdPermissions.ManageApplications,
        OpenIdPermissions.ManageScopes,
        OpenIdPermissions.ManageClientSettings,
        OpenIdPermissions.ManageServerSettings,
        OpenIdPermissions.ManageValidationSettings,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'OpenIdPermissions.ManageApplications'.")]
    public static readonly Permission ManageApplications
        = new("ManageApplications", "View, add, edit and remove the OpenID Connect applications.");

    [Obsolete("This will be removed in a future release. Instead use 'OpenIdPermissions.ManageScopes'.")]
    public static readonly Permission ManageScopes
        = new("ManageScopes", "View, add, edit and remove the OpenID Connect scopes.");

    [Obsolete("This will be removed in a future release. Instead use 'OpenIdPermissions.ManageClientSettings'.")]
    public static readonly Permission ManageClientSettings
        = new("ManageClientSettings", "View and edit the OpenID Connect client settings.");

    [Obsolete("This will be removed in a future release. Instead use 'OpenIdPermissions.ManageServerSettings'.")]
    public static readonly Permission ManageServerSettings
        = new("ManageServerSettings", "View and edit the OpenID Connect server settings.");

    [Obsolete("This will be removed in a future release. Instead use 'OpenIdPermissions.ManageValidationSettings'.")]
    public static readonly Permission ManageValidationSettings
        = new("ManageValidationSettings", "View and edit the OpenID Connect validation settings.");

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
