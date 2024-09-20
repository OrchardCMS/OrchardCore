using OrchardCore.OpenId.Core;
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
