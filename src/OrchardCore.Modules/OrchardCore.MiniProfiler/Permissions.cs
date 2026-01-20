using OrchardCore.Security.Permissions;

namespace OrchardCore.MiniProfiler;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ViewMiniProfilerOnFrontEnd = new("ViewMiniProfilerOnFrontEnd", "View Mini Profiler widget on front end pages");
    public static readonly Permission ViewMiniProfilerOnBackEnd = new("ViewMiniProfilerOnBackEnd", "View Mini Profiler widget on back end pages");

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
