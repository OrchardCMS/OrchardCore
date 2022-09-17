using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization;

public class PermissionLocalizer : IPermissionLocalizer
{
    private readonly IStringLocalizer S;

    public PermissionLocalizer(IStringLocalizer<PermissionLocalizer> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Permission Localizer(Permission permission)
    {
        if (permission == null)
        {
            throw new ArgumentNullException(nameof(permission));
        }

        if (String.IsNullOrEmpty(permission.Description))
        {
            return permission;
        }

        return new Permission(permission.Name, S[permission.Description], permission.ImpliedBy, permission.IsSecurityCritical);
    }
}
