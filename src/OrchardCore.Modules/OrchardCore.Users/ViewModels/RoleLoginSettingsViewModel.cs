using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class RoleLoginSettingsViewModel
{
    [BindNever]
    public bool EnableTwoFactorAuthentication { get; set; }

    public bool EnableTwoFactorAuthenticationForSpecificRoles { get; set; }

    public RoleEntry[] Roles { get; set; } = Array.Empty<RoleEntry>();
}
