using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Navigation;

namespace OrchardCore.Admin
{
    /// <summary>
    /// This filter inject a Navigation shape in the Navigation zone of the Layout
    /// for any ViewResult returned from an Admin controller.
    /// </summary>
    public class AdminMenuFilter : IAsyncResultFilter
    {
        private readonly INavigationManager _navigationManager;
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IShapeFactory _shapeFactory;

        public AdminMenuFilter(INavigationManager navigationManager,
            ILayoutAccessor layoutAccessor,
            IShapeFactory shapeFactory)
        {

            _navigationManager = navigationManager;
            _layoutAccessor = layoutAccessor;
            _shapeFactory = shapeFactory;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext filterContext, ResultExecutionDelegate next)
        {
            // Should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
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

            // Populate main nav
            var menuShape = await _shapeFactory.CreateAsync("Navigation",
                Arguments.From(new
                {
                    MenuName = "admin",
                    RouteData = filterContext.RouteData,
                }));

            dynamic layout = await _layoutAccessor.GetLayoutAsync();

            if (layout.Navigation is ZoneOnDemand zoneOnDemand)
            {
                await zoneOnDemand.AddAsync(menuShape);
            }
            else
            {
                layout.Navigation.Add(menuShape);
            }

            await next();
        }
    }
}
