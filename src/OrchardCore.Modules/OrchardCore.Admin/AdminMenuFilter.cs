using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Admin
{
    /// <summary>
    /// This filter inject a Navigation shape in the Navigation zone of the Layout
    /// for any ViewResult returned from an Admin controller.
    /// </summary>
    public class AdminMenuFilter : IAsyncResultFilter
    {
        private IShapeFactory shapeFactory;
        private ILayoutAccessor layoutAccessor;

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
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
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

            // Resolve services lazy if we got this far.
            shapeFactory ??= filterContext.HttpContext.RequestServices.GetRequiredService<IShapeFactory>();
            layoutAccessor ??= filterContext.HttpContext.RequestServices.GetRequiredService<ILayoutAccessor>();

            // Populate main nav
            var menuShape = await shapeFactory.CreateAsync("Navigation",
                Arguments.From(new
                {
                    MenuName = "admin",
                    RouteData = filterContext.RouteData,
                }));

            dynamic layout = await layoutAccessor.GetLayoutAsync();

            if (layout.Navigation is ZoneOnDemand zoneOnDemand)
            {
                await zoneOnDemand.AddAsync(menuShape);
            }
            else if (layout.Navigation is Shape shape)
            {
                shape.Add(menuShape);
            }

            await next();
        }
    }
}
