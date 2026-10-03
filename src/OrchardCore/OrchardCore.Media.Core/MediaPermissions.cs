using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public static class MediaPermissions
{
    public static readonly Permission ManageMediaFolder = new("ManageMediaFolder", new LocalizationSource("Manage All Media Folders", typeof(MediaPermissions)));

    public static readonly Permission ManageOthersMedia = new("ManageOthersMediaContent", new LocalizationSource("Manage Media For Others", typeof(MediaPermissions)), [ManageMediaFolder]);

    public static readonly Permission ManageOwnMedia = new("ManageOwnMediaContent", new LocalizationSource("Manage Own Media", typeof(MediaPermissions)), [ManageOthersMedia]);

    public static readonly Permission ManageAttachedMediaFieldsFolder = new("ManageAttachedMediaFieldsFolder", new LocalizationSource("Manage Attached Media Fields Folder", typeof(MediaPermissions)), [ManageMediaFolder]);

    public static readonly Permission ManageMedia = new("ManageMediaContent", new LocalizationSource("Manage Media", typeof(MediaPermissions)), [ManageOwnMedia, ManageAttachedMediaFieldsFolder]);

    public static readonly Permission UploadRestrictedMedia = new(
        "UploadRestrictedMedia",
        new LocalizationSource("Upload media file extensions requiring additional permission", typeof(MediaPermissions)),
        isSecurityCritical: true);

    public static readonly Permission ManageMediaProfiles = new("ManageMediaProfiles", new LocalizationSource("Manage Media Profiles", typeof(MediaPermissions)));

    public static readonly Permission ViewMediaOptions = new("ViewMediaOptions", new LocalizationSource("View Media Options", typeof(MediaPermissions)));

    public static readonly Permission ManageMediaApiSettings = new("ManageMediaApiSettings", new LocalizationSource("Manage Media API authentication settings", typeof(MediaPermissions)));

    public static readonly Permission ManageAssetCache = new("ManageAssetCache", new LocalizationSource("Manage Asset Cache Folder", typeof(MediaPermissions)));

    // Note: The ManageMediaFolder permission grants all access, so viewing must be implied by it too.
    public static readonly Permission ViewMedia = new("ViewMediaContent", new LocalizationSource("View media content in all folders", typeof(MediaPermissions)), new[] { ManageMediaFolder });

    public static readonly Permission ViewRootMedia = new("ViewRootMediaContent", new LocalizationSource("View media content in the root folder", typeof(MediaPermissions)), new[] { ViewMedia });

    public static readonly Permission ViewOthersMedia = new("ViewOthersMediaContent", new LocalizationSource("View others media content", typeof(MediaPermissions)), new[] { ManageMediaFolder });

    public static readonly Permission ViewOwnMedia = new("ViewOwnMediaContent", new LocalizationSource("View own media content", typeof(MediaPermissions)), new[] { ViewOthersMedia });
}
