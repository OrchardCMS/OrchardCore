using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.RemoteManagement;

public static class RemoteManagementPermissions
{
    public static readonly Permission AccessRemoteManagement = new(
        "AccessRemoteManagement",
        LocalizationSource.Create("Access remote management APIs", typeof(RemoteManagementPermissions)));

    public static readonly Permission ManageRemoteManagementConfiguration = new(
        "ManageRemoteManagementConfiguration",
        LocalizationSource.Create("Manage remote management configuration", typeof(RemoteManagementPermissions)));

}
