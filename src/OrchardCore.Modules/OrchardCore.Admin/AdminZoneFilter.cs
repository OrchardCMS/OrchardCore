using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace OrchardCore.Admin
{
    /// <summary>
    /// This filter makes an controller that starts with Admin and Razor Pages in /Pages/Admin folder behave as
    /// it had the <see cref="AdminAttribute"/>.
    /// </summary>
    public class AdminZoneFilter : IAsyncResourceFilter
    {
        private readonly string _adminUrlPrefix;

        public AdminZoneFilter(IOptions<AdminOptions> adminOptions)
        {
            _adminUrlPrefix = adminOptions.Value.AdminUrlPrefix;
        }
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
                if (page.ViewEnginePath.Contains(_adminUrlPrefix))
                {
                    AdminAttribute.Apply(context.HttpContext);
                }
            }

            return next();
        }
    }
}
