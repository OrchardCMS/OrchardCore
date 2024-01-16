using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenId;

public class Permissions : IPermissionProvider
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

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageApplications,
        ManageScopes,
        ManageClientSettings,
        ManageServerSettings,
        ManageValidationSettings,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
