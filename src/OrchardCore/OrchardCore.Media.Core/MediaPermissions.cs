using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public static class MediaPermissions
{
    public static readonly Permission ManageMediaFolder = new("ManageMediaFolder", LocalizationSource.Create("Manage All Media Folders", typeof(MediaPermissions)));

    public static readonly Permission ManageOthersMedia = new("ManageOthersMediaContent", LocalizationSource.Create("Manage Media For Others", typeof(MediaPermissions)), [ManageMediaFolder]);

    public static readonly Permission ManageOwnMedia = new("ManageOwnMediaContent", LocalizationSource.Create("Manage Own Media", typeof(MediaPermissions)), [ManageOthersMedia]);

    public static readonly Permission ManageAttachedMediaFieldsFolder = new("ManageAttachedMediaFieldsFolder", LocalizationSource.Create("Manage Attached Media Fields Folder", typeof(MediaPermissions)), [ManageMediaFolder]);

    public static readonly Permission ManageMedia = new("ManageMediaContent", LocalizationSource.Create("Manage Media", typeof(MediaPermissions)), [ManageOwnMedia, ManageAttachedMediaFieldsFolder]);

    public static readonly Permission UploadRestrictedMedia = new(
        "UploadRestrictedMedia",
        LocalizationSource.Create("Upload media file extensions requiring additional permission", typeof(MediaPermissions)),
        isSecurityCritical: true);

    public static readonly Permission ManageMediaProfiles = new("ManageMediaProfiles", LocalizationSource.Create("Manage Media Profiles", typeof(MediaPermissions)));

    public static readonly Permission ViewMediaOptions = new("ViewMediaOptions", LocalizationSource.Create("View Media Options", typeof(MediaPermissions)));

    public static readonly Permission ManageMediaApiSettings = new("ManageMediaApiSettings", LocalizationSource.Create("Manage Media API authentication settings", typeof(MediaPermissions)));

    public static readonly Permission ManageAssetCache = new("ManageAssetCache", LocalizationSource.Create("Manage Asset Cache Folder", typeof(MediaPermissions)));

    // Note: The ManageMediaFolder permission grants all access, so viewing must be implied by it too.
    public static readonly Permission ViewMedia = new("ViewMediaContent", LocalizationSource.Create("View media content in all folders", typeof(MediaPermissions)), new[] { ManageMediaFolder });

    public static readonly Permission ViewRootMedia = new("ViewRootMediaContent", LocalizationSource.Create("View media content in the root folder", typeof(MediaPermissions)), new[] { ViewMedia });

    public static readonly Permission ViewOthersMedia = new("ViewOthersMediaContent", LocalizationSource.Create("View others media content", typeof(MediaPermissions)), new[] { ManageMediaFolder });

    public static readonly Permission ViewOwnMedia = new("ViewOwnMediaContent", LocalizationSource.Create("View own media content", typeof(MediaPermissions)), new[] { ViewOthersMedia });
}
