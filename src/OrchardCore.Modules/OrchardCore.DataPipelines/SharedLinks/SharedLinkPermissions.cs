using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// The permissions of shared links.
/// </summary>
public static class SharedLinkPermissions
{
    /// <summary>
    /// List the shared links of every module, revoke and delete them.
    /// </summary>
    public static readonly Permission ManageSharedLinks = new("ManageSharedLinks", LocalizationSource.Create("Manage shared links", typeof(SharedLinkPermissions)));
}

public sealed class SharedLinkPermissionsProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        SharedLinkPermissions.ManageSharedLinks,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(_allPermissions);
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
