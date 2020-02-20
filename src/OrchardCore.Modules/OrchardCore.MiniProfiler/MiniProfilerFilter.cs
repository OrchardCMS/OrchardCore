using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;

namespace OrchardCore.MiniProfiler
{
    public class MiniProfilerFilter : IAsyncResultFilter
    {
        private ILayoutAccessor _layoutAccessor;
        private IShapeFactory _shapeFactory;

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Should only run on the front-end for a full view
            if ((context.Result is ViewResult || context.Result is PageResult) &&
                !AdminAttribute.IsApplied(context.HttpContext))
            {
                // Resolve scoped services lazy if we got this far.
                _layoutAccessor ??= context.HttpContext.RequestServices.GetRequiredService<ILayoutAccessor>();
                _shapeFactory ??= context.HttpContext.RequestServices.GetRequiredService<IShapeFactory>();

                dynamic layout = await _layoutAccessor.GetLayoutAsync();
                var footerZone = layout.Zones["Footer"];
                footerZone.Add(await _shapeFactory.CreateAsync("MiniProfiler"));
            }

            await next.Invoke();
        }
    }
}
