using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageViewEnginePathFilter : IAsyncPageFilter
    {
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var viewEnginePath = context.ActionDescriptor.ViewEnginePath;

            if (!viewEnginePath.Contains("/Pages/") || viewEnginePath.StartsWith("/Pages/"))
            {
                context.Result = new NotFoundResult();
                return;
            }

            var shellFeaturesManager = context.HttpContext.RequestServices.GetRequiredService<IShellFeaturesManager>();
            var moduleIds = (await shellFeaturesManager.GetEnabledFeaturesAsync()).Select(f => f.Extension.Id).Distinct();

            var moduleFolder = viewEnginePath.Substring(0, viewEnginePath.LastIndexOf("/Pages/"));
            var moduleId = moduleFolder.Substring(moduleFolder.LastIndexOf("/") + 1);

            if (!moduleIds.Contains(moduleId))
            {
                context.Result = new NotFoundResult();
            }
            else
            {
                await next();
            }
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }
    }
}