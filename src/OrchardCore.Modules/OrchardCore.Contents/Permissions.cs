using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents;

public sealed class Permissions : IPermissionProvider
{
    // Note - in code you should demand PublishContent, EditContent, or DeleteContent.
    // Do not demand the "Own" variations - those are applied automatically when you demand the main ones.

    // EditOwn is the permission that is ultimately required to create new content. See how the Create() method is implemented in the AdminController.

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.PublishContent'.")]
    public static readonly Permission PublishContent = CommonPermissions.PublishContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.PublishOwnContent'.")]
    public static readonly Permission PublishOwnContent = CommonPermissions.PublishOwnContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.EditContent'.")]
    public static readonly Permission EditContent = CommonPermissions.EditContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.EditOwnContent'.")]
    public static readonly Permission EditOwnContent = CommonPermissions.EditOwnContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.DeleteContent'.")]
    public static readonly Permission DeleteContent = CommonPermissions.DeleteContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.DeleteOwnContent'.")]
    public static readonly Permission DeleteOwnContent = CommonPermissions.DeleteOwnContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.ViewContent'.")]
    public static readonly Permission ViewContent = CommonPermissions.ViewContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.ViewOwnContent'.")]
    public static readonly Permission ViewOwnContent = CommonPermissions.ViewOwnContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.PreviewContent'.")]
    public static readonly Permission PreviewContent = CommonPermissions.PreviewContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.PreviewOwnContent'.")]
    public static readonly Permission PreviewOwnContent = CommonPermissions.PreviewOwnContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.CloneContent'.")]
    public static readonly Permission CloneContent = CommonPermissions.CloneContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.CloneOwnContent'.")]
    public static readonly Permission CloneOwnContent = CommonPermissions.CloneOwnContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.ListContent'.")]
    public static readonly Permission ListContent = CommonPermissions.ListContent;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.EditContentOwner'.")]
    public static readonly Permission EditContentOwner = CommonPermissions.EditContentOwner;

    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Contents.CommonPermissions.AccessContentApi'.")]
    public static readonly Permission AccessContentApi = new("AccessContentApi", "Access content via the api");

    private readonly IEnumerable<Permission> _readerPermissions =
    [
        CommonPermissions.ViewContent,
    ];

    private readonly IEnumerable<Permission> _allPermissions =
    [
        CommonPermissions.EditContent,
        CommonPermissions.EditOwnContent,
        CommonPermissions.PublishContent,
        CommonPermissions.PublishOwnContent,
        CommonPermissions.DeleteContent,
        CommonPermissions.DeleteOwnContent,
        CommonPermissions.ViewContent,
        CommonPermissions.ViewOwnContent,
        CommonPermissions.PreviewContent,
        CommonPermissions.PreviewOwnContent,
        CommonPermissions.CloneContent,
        CommonPermissions.CloneOwnContent,
        CommonPermissions.AccessContentApi,
        CommonPermissions.ListContent,
        CommonPermissions.EditContentOwner,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions =
            [
                CommonPermissions.PublishContent,
                CommonPermissions.EditContent,
                CommonPermissions.DeleteContent,
                CommonPermissions.PreviewContent,
                CommonPermissions.CloneContent,
                CommonPermissions.AccessContentApi,
                CommonPermissions.ListContent,
                CommonPermissions.EditContentOwner,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions =
            [
                CommonPermissions.PublishContent,
                CommonPermissions.EditContent,
                CommonPermissions.DeleteContent,
                CommonPermissions.PreviewContent,
                CommonPermissions.CloneContent,
                CommonPermissions.ListContent,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Author,
            Permissions =
            [
                CommonPermissions.PublishOwnContent,
                CommonPermissions.EditOwnContent,
                CommonPermissions.DeleteOwnContent,
                CommonPermissions.PreviewOwnContent,
                CommonPermissions.CloneOwnContent,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Contributor,
            Permissions =
            [
                CommonPermissions.EditOwnContent,
                CommonPermissions.PreviewOwnContent,
                CommonPermissions.CloneOwnContent,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Authenticated,
            Permissions = _readerPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Anonymous,
            Permissions = _readerPermissions,
        },
    ];
}
