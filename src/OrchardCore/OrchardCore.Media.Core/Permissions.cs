using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

[Obsolete("This class is deprecated and will be removed in a future version. Use OrchardCore.Media.MediaPermissions instead.")]
public static class Permissions
{
    public static readonly Permission ManageMediaFolder = MediaPermissions.ManageMediaFolder;

    public static readonly Permission ManageOthersMedia = MediaPermissions.ManageOthersMedia;

    public static readonly Permission ManageOwnMedia = MediaPermissions.ManageOwnMedia;

    public static readonly Permission ManageMedia = MediaPermissions.ManageMedia;

    public static readonly Permission ManageAttachedMediaFieldsFolder = MediaPermissions.ManageAttachedMediaFieldsFolder;

    public static readonly Permission ManageMediaProfiles = MediaPermissions.ManageMediaProfiles;

    public static readonly Permission ViewMediaOptions = MediaPermissions.ViewMediaOptions;
}
