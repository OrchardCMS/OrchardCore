using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Admin
{
    /// <summary>
    /// This filter inject a Navigation shape in the Navigation zone of the Layout
    /// for any ViewResult returned from an Admin controller.
    /// </summary>
    public class AdminMenuFilter : IAsyncResultFilter
    {
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IShapeFactory _shapeFactory;

        public AdminMenuFilter(ILayoutAccessor layoutAccessor,
            IShapeFactory shapeFactory)
        {
            _layoutAccessor = layoutAccessor;
            _shapeFactory = shapeFactory;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext filterContext, ResultExecutionDelegate next)
        {
            // Should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult) && !(filterContext.Result is PageResult))
            {
                await next();
                return;
            }

            // Should only run on the Admin
            if (!AdminAttribute.IsApplied(filterContext.HttpContext))
            {
                await next();
                return;
            }

            // Should only run for authenticated users
            if (!(filterContext.HttpContext.User?.Identity?.IsAuthenticated ?? false))
            {
                await next();
                return;
            }

            // Don't create the menu if the status code is 3xx
            var statusCode = filterContext.HttpContext.Response.StatusCode;
            if (statusCode >= 300 && statusCode < 400)
            {
                await next();
                return;
            }

            // Populate main nav
            var menuShape = await _shapeFactory.CreateAsync("Navigation",
                Arguments.From(new
                {
                    MenuName = "admin",
                    filterContext.RouteData,
                }));

            var layout = await _layoutAccessor.GetLayoutAsync();

            var navigation = layout.Zones["Navigation"];

            if (navigation is Shape shape)
            {
                await shape.AddAsync(menuShape);
            }

            await next();
        }
    }
}
