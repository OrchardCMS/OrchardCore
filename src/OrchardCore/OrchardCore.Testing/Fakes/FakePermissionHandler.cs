using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security;

namespace OrchardCore.Testing.Fakes;

internal class FakePermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly HashSet<string> _permissionNames;

    public FakePermissionHandler(string[] permissionNames)
    {
        _permissionNames = new HashSet<string>(permissionNames, StringComparer.OrdinalIgnoreCase);
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (_permissionNames.Contains(requirement.Permission.Name))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
