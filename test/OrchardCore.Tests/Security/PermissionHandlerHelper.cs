using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Security
{
    public static class PermissionHandlerHelper
    {
        public static AuthorizationHandlerContext CreateTestAuthorizationHandlerContext(Permission required, string[] allowed = null, bool authenticated = false)
        {
            var identity = authenticated ? new ClaimsIdentity("Test") : new ClaimsIdentity();

            if (allowed != null)
            {
                foreach (var permissionName in allowed)
                {
                    var permission = new Permission(permissionName);
                    identity.AddClaim(permission);
                }

            }

            var principal = new ClaimsPrincipal(identity);

            return new AuthorizationHandlerContext(
                new[] { new PermissionRequirement(required) },
                principal,
                null);
        }

        public static async Task SuccessAsync(this AuthorizationHandlerContext context, params string[] permissionNames)
        {
            var handler = new FakePermissionHandler(permissionNames);
            await handler.HandleAsync(context);
        }

        private class FakePermissionHandler : AuthorizationHandler<PermissionRequirement>
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
    }
}
