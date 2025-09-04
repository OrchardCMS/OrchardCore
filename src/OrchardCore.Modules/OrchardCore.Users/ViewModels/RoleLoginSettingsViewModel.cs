namespace OrchardCore.Users.ViewModels;

public class RoleLoginSettingsViewModel
{
    public bool RequireTwoFactorAuthenticationForSpecificRoles { get; set; }

    public RoleEntry[] Roles { get; set; } = [];
}
