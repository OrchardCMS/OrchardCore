using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Layout;

namespace OrchardCore.DisplayManagement.Razor
{
    /// <summary>
    /// Inject an instance of the theme layout <see cref="IShape"/> in the HttpContext items such that
    /// a View can reuse it when it's executed.
    /// </summary>
    public class ThemeLayoutViewResultFilter : IAsyncResultFilter, IViewResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await OnResultExecutionAsync(context.HttpContext);
            await next();
        }

        // Used when we create fake view and action contexts.
        public Task OnResultExecutionAsync(ActionContext context)
        {
            return OnResultExecutionAsync(context.HttpContext);
        }

        private async Task OnResultExecutionAsync(HttpContext context)
        {
            if (!context.Items.ContainsKey(typeof(IShape)))
            {
                var layoutAccessor = context.RequestServices.GetService<ILayoutAccessor>();

                if (layoutAccessor != null)
                {
                    context.Items.Add(typeof(IShape), await layoutAccessor.GetLayoutAsync());
                }
            }
        }
    }
}
