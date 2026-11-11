using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Jobs;

public sealed class JobsPermissions : IPermissionProvider
{
    public static readonly Permission ManageJobs = new("ManageJobs", LocalizationSource.Create<JobsPermissions>("Manage jobs: run them, cancel and delete their runs, and change their schedules"));

    public static readonly Permission ViewJobs = new("ViewJobs", LocalizationSource.Create<JobsPermissions>("View jobs, their runs and their schedules"), [ManageJobs]);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageJobs,
        ViewJobs,
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
