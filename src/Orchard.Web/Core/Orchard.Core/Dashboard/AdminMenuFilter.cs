using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Routing;
using Orchard.DependencyInjection;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Admin;
using Orchard.DisplayManagement.Layout;
using Orchard.Environment.Navigation;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Core.Dashboard
{
    public class AdminMenuFilterModule : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<MvcOptions>(setup =>
            {
                setup.Filters.Add(typeof(AdminMenuFilter));
            });
        }
    }

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
                ;

            _layoutAccessor.GetLayout().Navigation.Add(menuShape);
            
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}