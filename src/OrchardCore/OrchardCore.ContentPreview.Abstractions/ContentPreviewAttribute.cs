using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.ContentPreview
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    /// <summary>
    /// When applied to a preview action or controller or a page model, marks the request as being in preview mode.
    /// </summary>
    public class ContentPreviewAttribute : ActionFilterAttribute
    {
        public ContentPreviewAttribute()
        {
            // Ordered to call 'Apply' before any global filter, with a default order of 0, might call 'IsApplied'.
            Order = -1000;
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Apply(context.HttpContext);

            return base.OnActionExecutionAsync(context, next);
        }

        public static void Apply(HttpContext context)
        {
            // The value isn't important, it's just a marker object
            context.Items[typeof(ContentPreviewAttribute)] = null;
        }

        public static bool IsApplied(HttpContext context)
        {
            return context.Items.TryGetValue(typeof(ContentPreviewAttribute), out var value);
        }
    }
}
