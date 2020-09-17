using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageViewEnginePathFilter : IAsyncPageFilter
    {
        private readonly bool _found;

        public ModularPageViewEnginePathFilter(bool found)
        {
            _found = found;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (!_found)
            {
                context.Result = new NotFoundResult();
                return;
            }

            await next();
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }
    }
}
