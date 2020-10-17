using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Admin;

namespace OrchardCore.Upgrade.Services
{
    /// <summary>
    /// Abstract class which intercepts any request to the admin site to notify the user an upgrade is required.
    /// </summary>
    public abstract class UpgradeFilter : ActionFilterAttribute, IAsyncPageFilter
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (AdminAttribute.IsApplied(context.HttpContext))
            {
                Notify();
            }

            await base.OnActionExecutionAsync(context, next);
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (AdminAttribute.IsApplied(context.HttpContext))
            {
                Notify();
            }

            // Do post work.
            await next.Invoke();
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        protected abstract void Notify();
    }
}
