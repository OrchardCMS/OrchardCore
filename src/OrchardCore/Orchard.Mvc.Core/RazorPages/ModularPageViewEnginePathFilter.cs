using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell;

namespace Orchard.Mvc.RazorPages
{
    public class ModularPageViewEnginePathFilter : IAsyncPageFilter
    {
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var shellFeaturesManager = context.HttpContext.RequestServices.GetRequiredService<IShellFeaturesManager>();

            var moduleIds = (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Select(f => f.Extension.Id).Distinct();

            var moduleId = context.ActionDescriptor.ViewEnginePath.Substring(1,
                context.ActionDescriptor.ViewEnginePath.IndexOf("/Pages/") - 1);

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