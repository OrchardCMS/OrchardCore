using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public static class MediaPermissions
{
    public static readonly Permission ManageMediaFolder = new("ManageMediaFolder", LocalizedString.Create("Manage All Media Folders", typeof(MediaPermissions)));

    public static readonly Permission ManageOthersMedia = new("ManageOthersMediaContent", LocalizedString.Create("Manage Media For Others", typeof(MediaPermissions)), [ManageMediaFolder]);

    public static readonly Permission ManageOwnMedia = new("ManageOwnMediaContent", LocalizedString.Create("Manage Own Media", typeof(MediaPermissions)), [ManageOthersMedia]);

    public static readonly Permission ManageAttachedMediaFieldsFolder = new("ManageAttachedMediaFieldsFolder", LocalizedString.Create("Manage Attached Media Fields Folder", typeof(MediaPermissions)), [ManageMediaFolder]);

    public static readonly Permission ManageMedia = new("ManageMediaContent", LocalizedString.Create("Manage Media", typeof(MediaPermissions)), [ManageOwnMedia, ManageAttachedMediaFieldsFolder]);

    public static readonly Permission UploadRestrictedMedia = new(
        "UploadRestrictedMedia",
        LocalizedString.Create("Upload media file extensions requiring additional permission", typeof(MediaPermissions)),
        isSecurityCritical: true);

    public static readonly Permission ManageMediaProfiles = new("ManageMediaProfiles", LocalizedString.Create("Manage Media Profiles", typeof(MediaPermissions)));

    public static readonly Permission ViewMediaOptions = new("ViewMediaOptions", LocalizedString.Create("View Media Options", typeof(MediaPermissions)));

    public static readonly Permission ManageMediaApiSettings = new("ManageMediaApiSettings", LocalizedString.Create("Manage Media API authentication settings", typeof(MediaPermissions)));

    public static readonly Permission ManageAssetCache = new("ManageAssetCache", LocalizedString.Create("Manage Asset Cache Folder", typeof(MediaPermissions)));

    // Note: The ManageMediaFolder permission grants all access, so viewing must be implied by it too.
    public static readonly Permission ViewMedia = new("ViewMediaContent", LocalizedString.Create("View media content in all folders", typeof(MediaPermissions)), new[] { ManageMediaFolder });

    public static readonly Permission ViewRootMedia = new("ViewRootMediaContent", LocalizedString.Create("View media content in the root folder", typeof(MediaPermissions)), new[] { ViewMedia });

    public static readonly Permission ViewOthersMedia = new("ViewOthersMediaContent", LocalizedString.Create("View others media content", typeof(MediaPermissions)), new[] { ManageMediaFolder });

    public static readonly Permission ViewOwnMedia = new("ViewOwnMediaContent", LocalizedString.Create("View own media content", typeof(MediaPermissions)), new[] { ViewOthersMedia });
}
