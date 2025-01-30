using OrchardCore.Security.Permissions;

namespace OrchardCore.Seo;

public sealed class SeoPermissionProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        SeoPermissions.ManageSeoSettings,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'SeoPermissions.ManageSeoSettings'.")]
    public static readonly Permission ManageSeoSettings = SeoConstants.ManageSeoSettings;

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
