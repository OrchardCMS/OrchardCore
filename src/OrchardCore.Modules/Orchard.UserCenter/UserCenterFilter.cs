using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Orchard.UserCenter
{
    /// <summary>
    /// Intercepts any request to check whether it applies to the admin site.
    /// If so it marks the request as such and ensures the user as the right to access it.
    /// </summary>
    public class UserCenterFilter : ActionFilterAttribute
    {
        private readonly IAuthorizationService _authorizationService;

        public UserCenterFilter(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (UserCenterAttribute.IsApplied(context.HttpContext) || IsNameUserCenter(context))
            {
                var authorized = await _authorizationService.AuthorizeAsync(context.HttpContext.User, Permissions.AccessUserCenterPanel);

                if (!authorized)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }

        private bool IsNameUserCenter(ActionExecutingContext context)
        {
            return string.Equals(context.Controller.GetType().Name, "UserCenter", StringComparison.OrdinalIgnoreCase);
        }
    }
}
