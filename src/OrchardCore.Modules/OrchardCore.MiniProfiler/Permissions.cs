using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.MiniProfiler;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ViewMiniProfilerOnFrontEnd = new("ViewMiniProfilerOnFrontEnd", "View Mini Profiler widget on front end pages");
    public static readonly Permission ViewMiniProfilerOnBackEnd = new("ViewMiniProfilerOnBackEnd", "View Mini Profiler widget on back end pages");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ViewMiniProfilerOnFrontEnd,
        ViewMiniProfilerOnBackEnd,
    ];
}
