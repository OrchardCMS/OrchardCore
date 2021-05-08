using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
                context.Fail();
            }

            return Task.CompletedTask;
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
