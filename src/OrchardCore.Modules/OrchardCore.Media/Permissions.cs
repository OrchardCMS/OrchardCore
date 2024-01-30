using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageMediaFolder = new("ManageMediaFolder", "Manage All Media Folders");
    public static readonly Permission ManageOthersMedia = new("ManageOthersMediaContent", "Manage Media For Others", new[] { ManageMediaFolder });
    public static readonly Permission ManageOwnMedia = new("ManageOwnMediaContent", "Manage Own Media", new[] { ManageOthersMedia });
    public static readonly Permission ManageMedia = new("ManageMediaContent", "Manage Media", new[] { ManageOwnMedia });
    public static readonly Permission ManageAttachedMediaFieldsFolder = new("ManageAttachedMediaFieldsFolder", "Manage Attached Media Fields Folder", new[] { ManageMediaFolder });
    public static readonly Permission ManageMediaProfiles = new("ManageMediaProfiles", "Manage Media Profiles");
    public static readonly Permission ViewMediaOptions = new("ViewMediaOptions", "View Media Options");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageMedia,
        ManageMediaFolder,
        ManageOthersMedia,
        ManageOwnMedia,
        ManageAttachedMediaFieldsFolder,
        ManageMediaProfiles,
        ViewMediaOptions,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        ManageOwnMedia,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions =
            [
                ManageMediaFolder,
                ManageMediaProfiles,
                ViewMediaOptions,
            ],
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions =
            [
                ManageMedia,
                ManageOwnMedia,
            ],
        },
        new PermissionStereotype
        {
            Name = "Author",
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = "Contributor",
            Permissions = _generalPermissions,
        },
    ];
}
