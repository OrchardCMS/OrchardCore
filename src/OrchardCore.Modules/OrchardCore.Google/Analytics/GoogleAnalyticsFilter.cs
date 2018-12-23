using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;

namespace OrchardCore.Google.Analytics
{
    public class GoogleAnalyticsFilter : IAsyncResultFilter
    {
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IShapeFactory _shapeFactory;

        public GoogleAnalyticsFilter(
            ILayoutAccessor layoutAccessor,
            IShapeFactory shapeFactory)
        {
            _layoutAccessor = layoutAccessor;
            _shapeFactory = shapeFactory;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Should only run on the front-end for a full view
            if ((context.Result is ViewResult || context.Result is PageResult) &&
                !AdminAttribute.IsApplied(context.HttpContext))
            {
                dynamic layout = await _layoutAccessor.GetLayoutAsync();
                layout.Zones["Header"].Add(await _shapeFactory.CreateAsync("gtag"));
            }
            await next.Invoke();
        }
    }
}
