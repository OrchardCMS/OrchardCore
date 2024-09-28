using System.ComponentModel;

namespace OrchardCore.Users.Models;

public class LoginSettings
{
    public bool UseSiteTheme { get; set; }

    [Obsolete("This property is no longer used and will be removed in the next major release. Instead use ExternalUserLoginSettings.NoPassword")]
    public bool UseExternalProviderIfOnlyOneDefined { get; set; }

    public bool DisableLocalLogin { get; set; }

    [Obsolete("This property is no longer used and will be removed in the next major release. Instead use ExternalUserRoleLoginSettings.SyncRolesScript")]
    public bool UseScriptToSyncRoles { get; set; }

    [Obsolete("This property is no longer used and will be removed in the next major release. Instead use ExternalUserRoleLoginSettings.SyncRolesScript")]
    public string SyncRolesScript { get; set; }

    public bool AllowChangingUsername { get; set; }

    public bool AllowChangingEmail { get; set; }

    [DefaultValue(true)]
    public bool AllowChangingPhoneNumber { get; set; } = true;
}
