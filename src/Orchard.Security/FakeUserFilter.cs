using Microsoft.AspNetCore.Mvc.Filters;
using Orchard.DependencyInjection;
using System.Security.Claims;

namespace Orchard.Security
{
    [ScopedComponent(typeof(IFilterMetadata))]
    public class FakeUserFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //var validPrincipal = new ClaimsPrincipal(
            //    new ClaimsIdentity( new[] {
            //        new Claim(ClaimTypes.Name, "Admin"),
            //        new Claim(ClaimTypes.Role, "Administrator"),
            //}));

            //context.HttpContext.User = validPrincipal;
        }
    }
}
