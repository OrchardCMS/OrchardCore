using OrchardCore.Security.Permissions;

namespace OrchardCore.RemoteManagement;

public static class RemoteManagementPermissions
{
    public static readonly Permission AccessRemoteManagement = new(
        "AccessRemoteManagement",
        "Access remote management APIs");

    public static readonly Permission ManageRemoteManagementConfiguration = new(
        "ManageRemoteManagementConfiguration",
        "Manage remote management configuration");

}
