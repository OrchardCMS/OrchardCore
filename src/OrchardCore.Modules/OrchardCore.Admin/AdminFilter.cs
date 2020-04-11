using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Admin
{
    /// <summary>
    /// Intercepts any request to check whether it applies to the admin site.
    /// If so it marks the request as such and ensures the user as the right to access it.
    /// </summary>
    public class AdminFilter : ActionFilterAttribute, IAsyncPageFilter
    {
        private readonly IAuthorizationService _authorizationService;

        // constructor, set authorization service
        public AdminFilter(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        // callback on action execution
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // if the context is not set
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // if authorizatin failed
            if (!await AuthorizeAsync(context.HttpContext))
            {
                context.Result = context.HttpContext.User.Identity.IsAuthenticated ? (IActionResult)new ForbidResult() : (IActionResult)new ChallengeResult();
                return;
            }

            await base.OnActionExecutionAsync(context, next);
        }

        // callback on page handler execution
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            // if the context is not set
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!await AuthorizeAsync(context.HttpContext))
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Do post work.
            await next.Invoke();
        }

        // callback on page handler selection
        // \return completed task
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        // authorization with permission to access the admin panel
        // \return result (success) in the completed task
        private Task<bool> AuthorizeAsync(Microsoft.AspNetCore.Http.HttpContext context)
        {
            if (AdminAttribute.IsApplied(context))
            {
                return _authorizationService.AuthorizeAsync(context.User, Permissions.AccessAdminPanel);
            }

            return Task.FromResult(true);
        }
    }
}
