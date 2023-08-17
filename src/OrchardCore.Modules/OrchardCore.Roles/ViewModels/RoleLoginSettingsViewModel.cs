using System;

namespace OrchardCore.Roles.ViewModels;

public class RoleLoginSettingsViewModel
{
    public bool EnableTwoFactorAuthenticationForSpecificRoles { get; set; }

    public RoleEntry[] Roles { get; set; } = Array.Empty<RoleEntry>();
}
