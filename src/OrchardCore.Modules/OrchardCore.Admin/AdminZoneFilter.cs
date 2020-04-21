using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchardCore.Admin
{
    /// <summary>
    /// This filter makes an controller that starts with Admin and Razor Pages in /Pages/Admin folder behave as
    /// it had the <see cref="AdminAttribute"/>.
    /// </summary>
    public class AdminZoneFilter : IAsyncResourceFilter
    {
        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor action)
            {
                if (action.ControllerName == "Admin")
                {
                    AdminAttribute.Apply(context.HttpContext);
                }
            }
            else if (context.ActionDescriptor is PageActionDescriptor page)
            {
                if (page.ViewEnginePath.Contains("/Admin/"))
                {
                    AdminAttribute.Apply(context.HttpContext);
                }
            }

            return next();
        }
    }
}
