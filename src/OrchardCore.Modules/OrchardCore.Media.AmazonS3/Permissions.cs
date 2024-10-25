using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.AmazonS3;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ViewAmazonS3MediaOptions = new("ViewAmazonS3MediaOptions", "View Amazon S3 Media Options");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ViewAmazonS3MediaOptions,
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
