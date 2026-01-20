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
    public static readonly Permission ManageApplications = OpenIdPermissions.ManageApplications;

    [Obsolete("This will be removed in a future release. Instead use 'OpenIdPermissions.ManageScopes'.")]
    public static readonly Permission ManageScopes = OpenIdPermissions.ManageScopes;

    [Obsolete("This will be removed in a future release. Instead use 'OpenIdPermissions.ManageClientSettings'.")]
    public static readonly Permission ManageClientSettings = OpenIdPermissions.ManageClientSettings;

    [Obsolete("This will be removed in a future release. Instead use 'OpenIdPermissions.ManageServerSettings'.")]
    public static readonly Permission ManageServerSettings = OpenIdPermissions.ManageServerSettings;

    [Obsolete("This will be removed in a future release. Instead use 'OpenIdPermissions.ManageValidationSettings'.")]
    public static readonly Permission ManageValidationSettings = OpenIdPermissions.ManageValidationSettings;

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
