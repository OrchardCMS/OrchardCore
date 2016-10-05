using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Layout;
using Orchard.Environment.Navigation;

namespace Orchard.Admin
{
    public class AdminMenuFilter : IResultFilter
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

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // Should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
            {
                return;
            }

            // Should only run on the Admin
            if (!AdminAttribute.IsApplied(filterContext.HttpContext))
            {
                return;
            }

            // Populate main nav
            IShape menuShape = _shapeFactory.Create("Menu",
                Arguments.From(new
                {
                    MenuName = "admin",
                    RouteData = filterContext.RouteData,
                }));

            // Enable shape caching
            menuShape.Metadata
                .Cache("menu-admin")
                .AddContext("user.roles")
                .AddDependency("features")
                .AddDependency("types") // This should go in a Displaying Shape event in the Contents module
                ;

            _layoutAccessor.GetLayout().Navigation.Add(menuShape);

        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}