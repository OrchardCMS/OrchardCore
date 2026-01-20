using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Admin
{
    /// <summary>
    /// When applied to an action or a controller or a page model, intercepts any request to check whether it applies to the admin site.
    /// If so it marks the request as such and ensures the user has the right to access it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AdminAttribute : Attribute, IAsyncResourceFilter
    {
        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            Apply(context.HttpContext);

            return next();
        }

        public static void Apply(HttpContext context)
        {
            // The value isn't important, it's just a marker object
            context.Items[typeof(AdminAttribute)] = null;
        }

        public static bool IsApplied(HttpContext context) => context.Items.TryGetValue(typeof(AdminAttribute), out _);
    }
}
