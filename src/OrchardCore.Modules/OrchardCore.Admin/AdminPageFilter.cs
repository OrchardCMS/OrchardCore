using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace OrchardCore.Admin
{
    public class AdminPageFilter : IAsyncPageFilter
    {
        private readonly IAuthorizationService _authorizationService;

          public AdminPageFilter(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (AdminAttribute.IsApplied(context.HttpContext))
            {
                var authorized = await _authorizationService.AuthorizeAsync(context.HttpContext.User, Permissions.AccessAdminPanel);

                if (!authorized)
                {
                    context.Result = new ChallengeResult();   
                    return;         
                }                
            }

            // Do post work.
            await next.Invoke();
        }
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }
    }
}