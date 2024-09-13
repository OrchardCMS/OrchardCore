namespace OrchardCore.Users.Models;

public class ExternalUserRoleLoginSettings
{
    public bool UseScriptToSyncRoles { get; set; }

    public string SyncRolesScript { get; set; }
}
