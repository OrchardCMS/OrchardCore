using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class RoleLoginSettingsViewModel
{
    public bool RequireTwoFactorAuthenticationForSpecificRoles { get; set; }

    public RoleEntry[] Roles { get; set; } = Array.Empty<RoleEntry>();
}
