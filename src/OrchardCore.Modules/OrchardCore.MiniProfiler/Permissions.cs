using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.MiniProfiler;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ViewMiniProfilerOnFrontEnd = new("ViewMiniProfilerOnFrontEnd", LocalizedString.Create("View Mini Profiler widget on front end pages", typeof(Permissions)));
    public static readonly Permission ViewMiniProfilerOnBackEnd = new("ViewMiniProfilerOnBackEnd", LocalizedString.Create("View Mini Profiler widget on back end pages", typeof(Permissions)));

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ViewMiniProfilerOnFrontEnd,
        ViewMiniProfilerOnBackEnd,
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
