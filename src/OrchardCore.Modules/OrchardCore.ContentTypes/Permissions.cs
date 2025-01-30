using OrchardCore.Contents;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        ContentTypesPermissions.ViewContentTypes,
        ContentTypesPermissions.EditContentTypes,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'ContentTypesPermissions.ViewContentTypes'.")]
    public static readonly Permission ViewContentTypes = new("ViewContentTypes", "View content types.");

    [Obsolete("This will be removed in a future release. Instead use 'ContentTypesPermissions.EditContentTypes'.")]
    public static readonly Permission EditContentTypes = new("EditContentTypes", "Edit content types.", isSecurityCritical: true);

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
