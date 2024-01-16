using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentManagement.GraphQL;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ApiViewContent = new("ApiViewContent", "Access view content endpoints");

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
        // new PermissionStereotype
        // {
        //     Name = "Editor"
        // },
        // new PermissionStereotype
        // {
        //     Name = "Moderator"
        // },
        // new PermissionStereotype
        // {
        //     Name = "Author"
        // },
        // new PermissionStereotype
        // {
        //     Name = "Contributor"
        // },
        // new PermissionStereotype
        // {
        //     Name = "Authenticated"
        // },
        // new PermissionStereotype
        // {
        //     Name = "Anonymous"
        // },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ApiViewContent,
    ];
}
