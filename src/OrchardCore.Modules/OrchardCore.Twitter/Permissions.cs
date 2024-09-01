using OrchardCore.Security.Permissions;

namespace OrchardCore.Twitter;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTwitter = new("ManageTwitter", "Manage X (Twitter) settings");

    public static readonly Permission ManageTwitterSignin = new("ManageTwitterSignin", "Manage Sign in with X (Twitter) settings");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageTwitter,
        ManageTwitterSignin,
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
