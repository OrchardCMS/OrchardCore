using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ViewContentTypes = new("ViewContentTypes", "View content types.");
    public static readonly Permission EditContentTypes = new("EditContentTypes", "Edit content types.", isSecurityCritical: true);

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ViewContentTypes,
        EditContentTypes
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
