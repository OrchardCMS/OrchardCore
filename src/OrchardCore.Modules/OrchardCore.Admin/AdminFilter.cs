using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Admin;

/// <summary>
/// Intercepts any request to check whether it applies to the admin site.
/// If so it marks the request as such and ensures the user as the right to access it.
/// </summary>
public sealed class AdminFilter : ActionFilterAttribute, IAsyncPageFilter
{
    private readonly IAuthorizationService _authorizationService;

    public AdminFilter(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!await AuthorizeAsync(context.HttpContext))
        {
            context.Result = context.HttpContext.User?.Identity?.IsAuthenticated ?? false
                ? new ForbidResult()
                : new ChallengeResult();
            return;
        }

        await base.OnActionExecutionAsync(context, next);
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!await AuthorizeAsync(context.HttpContext))
        {
            context.Result = new ChallengeResult();
            return;
        }

        // Do post work.
        await next.Invoke();
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    private Task<bool> AuthorizeAsync(Microsoft.AspNetCore.Http.HttpContext context)
    {
        if (AdminAttribute.IsApplied(context))
        {
            return _authorizationService.AuthorizeAsync(context.User, AdminPermissions.AccessAdminPanel);
        }

        return Task.FromResult(true);
    }
}
