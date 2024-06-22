using System;
using System.Collections.Generic;

namespace OrchardCore.Security.Services;

public static class RoleHelper
{
    public static readonly HashSet<string> SystemRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        OrchardCoreConstants.Roles.Anonymous,
        OrchardCoreConstants.Roles.Authenticated,
    };
}
