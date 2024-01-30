using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.AmazonS3;

public class Permissions : IPermissionProvider
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
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];
}
