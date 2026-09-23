using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Twitter;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTwitter = new("ManageTwitter", LocalizedString.Create("Manage X (Twitter) settings", typeof(Permissions)));

    public static readonly Permission ManageTwitterSignin = new("ManageTwitterSignin", LocalizedString.Create("Manage Sign in with X (Twitter) settings", typeof(Permissions)));

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
