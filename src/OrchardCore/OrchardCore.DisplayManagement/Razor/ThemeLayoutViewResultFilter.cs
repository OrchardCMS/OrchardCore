using System.Threading.Tasks;
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
    public class ThemeLayoutViewResultFilter : IAsyncViewResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await OnResultExecutionAsync(context);
            await next();
        }

        // Used as a service when we create a fake 'ActionContext'.
        public async Task OnResultExecutionAsync(ActionContext context)
        {
            if (!context.HttpContext.Items.ContainsKey(typeof(IShape)))
            {
                var layoutAccessor = context.HttpContext.RequestServices.GetService<ILayoutAccessor>();

                if (layoutAccessor != null)
                {
                    context.HttpContext.Items.Add(typeof(IShape), await layoutAccessor.GetLayoutAsync());
                }
            }
        }
    }
}
