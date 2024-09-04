using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public static class Permissions
{
    public static readonly Permission ManageMediaFolder = new("ManageMediaFolder", "Manage All Media Folders");

    public static readonly Permission ManageOthersMedia = new("ManageOthersMediaContent", "Manage Media For Others", [ManageMediaFolder]);

    public static readonly Permission ManageOwnMedia = new("ManageOwnMediaContent", "Manage Own Media", [ManageOthersMedia]);

    public static readonly Permission ManageMedia = new("ManageMediaContent", "Manage Media", [ManageOwnMedia]);

    public static readonly Permission ManageAttachedMediaFieldsFolder = new("ManageAttachedMediaFieldsFolder", "Manage Attached Media Fields Folder", [ManageMediaFolder]);

    public static readonly Permission ManageMediaProfiles = new("ManageMediaProfiles", "Manage Media Profiles");

    public static readonly Permission ViewMediaOptions = new("ViewMediaOptions", "View Media Options");
}
