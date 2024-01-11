using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Apis.Context
{
    internal class PermissionContextAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly PermissionsContext _permissionsContext;

        // Used for http based graphql tests, marries a permission context to a request
        public PermissionContextAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IDictionary<string, PermissionsContext> permissionsContexts)
        {
            _permissionsContext = new PermissionsContext();

            if (httpContextAccessor.HttpContext == null)
            {
                return;
            }

            var requestContext = httpContextAccessor.HttpContext.Request;

            if (requestContext?.Headers.ContainsKey("PermissionsContext") == true &&
                permissionsContexts.TryGetValue(requestContext.Headers["PermissionsContext"], out var permissionsContext))
            {
                _permissionsContext = permissionsContext;
            }
        }

        // Used for static graphql test; passes a permissionsContext directly
        public PermissionContextAuthorizationHandler(PermissionsContext permissionsContext)
        {
            _permissionsContext = permissionsContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var permissions = (_permissionsContext.AuthorizedPermissions ?? Enumerable.Empty<Permission>()).ToList();

            if (!_permissionsContext.UsePermissionsContext)
            {
                context.Succeed(requirement);
            }
            else if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
            else
            {
                var grantingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                GetGrantingNamesInternal(requirement.Permission, grantingNames);

                // SiteOwner permission grants them all
                grantingNames.Add(StandardPermissions.SiteOwner.Name);

                if (permissions.Any(p => grantingNames.Contains(p.Name)))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }

            return Task.CompletedTask;
        }

        private void GetGrantingNamesInternal(Permission permission, HashSet<string> stack)
        {
            // The given name is tested
            stack.Add(permission.Name);

            // Iterate implied permissions to grant, it present
            if (permission.ImpliedBy != null && permission.ImpliedBy.Any())
            {
                foreach (var impliedBy in permission.ImpliedBy)
                {
                    // Avoid potential recursion
                    if (impliedBy == null || stack.Contains(impliedBy.Name))
                    {
                        continue;
                    }

                    // Otherwise accumulate the implied permission names recursively
                    GetGrantingNamesInternal(impliedBy, stack);
                }
            }
        }
    }

    public class PermissionsContext
    {
        public IEnumerable<Permission> AuthorizedPermissions { get; set; } = Enumerable.Empty<Permission>();

        public bool UsePermissionsContext { get; set; } = false;
    }

    internal class StubIdentity : IIdentity
    {
        public string AuthenticationType => "TEST TEST";

        public bool IsAuthenticated => true;

        public string Name => "Mr Robot";
    }
}
