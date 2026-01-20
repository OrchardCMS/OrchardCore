using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ViewContentTypes = new("ViewContentTypes", "View content types.");
    public static readonly Permission EditContentTypes = new("EditContentTypes", "Edit content types.", isSecurityCritical: true);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ViewContentTypes,
        EditContentTypes,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];
}
