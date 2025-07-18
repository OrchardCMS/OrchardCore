using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenApi;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions = [OpenApiPermissions.ApiViewContent];

    public Task<IEnumerable<Permission>> GetPermissionsAsync() => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
        [
            new PermissionStereotype
            {
                Name = OrchardCoreConstants.Roles.Administrator,
                Permissions = [OpenApiPermissions.ApiViewContent],
            },
        ];
}
