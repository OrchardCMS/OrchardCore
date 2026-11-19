using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Dashboards;

public sealed class UserDashboardPermissions : IPermissionProvider
{
    /// <summary>
    /// The name of the dashboard every user has for themselves.
    /// </summary>
    public static readonly LocalizationSource DisplayName = LocalizationSource.Create<UserDashboardPermissions>("My Dashboard");

    public static readonly Permission ManageOwnDashboard = new("ManageOwnDashboard", LocalizationSource.Create<UserDashboardPermissions>("Manage their own dashboard"));

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageOwnDashboard,
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
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Moderator,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Author,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Contributor,
            Permissions = _allPermissions,
        },
    ];
}
