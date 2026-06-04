using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Navigation;

public class NavigationShapes : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Navigation")
            .OnDisplaying(displaying =>
            {
                var menu = displaying.Shape;
                var menuName = menu.GetProperty<string>("MenuName");

                menu.Classes.Add("menu-" + menuName.HtmlClassify());
                menu.Classes.Add("menu");
                menu.Metadata.Alternates.AddRange(NavigationAlternatesFactory.GetNavigationAlternates(menuName));
            })
            .OnProcessing(async context =>
            {
                var menu = context.Shape;
                var menuName = menu.GetProperty<string>("MenuName");

                // Menu population is executed when processing the shape so that its value
                // can be cached. IShapeDisplayEvents is called before the ShapeDescriptor
                // events and thus this code can be cached.

                if (menu is Shape shape && shape.HasItems)
                {
                    return;
                }

                var viewContextAccessor = context.ServiceProvider.GetRequiredService<ViewContextAccessor>();
                var viewContext = viewContextAccessor.ViewContext;
                var navigationManagers = context.ServiceProvider.GetServices<INavigationManager>();
                var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

                foreach (var navigationManager in navigationManagers)
                {
                    var menuItems = await navigationManager.BuildMenuAsync(menuName, viewContext);
                    var httpContext = httpContextAccessor.HttpContext;

                    if (httpContext != null)
                    {
                        // adding query string parameters
                        var route = menu.GetProperty<RouteData>("RouteData");
                        var routeData = new RouteValueDictionary(route.Values);
                        var query = httpContext.Request.Query;

                        if (query != null)
                        {
                            foreach (var pair in query)
                            {
                                if (pair.Key != null && !routeData.ContainsKey(pair.Key))
                                {
                                    routeData[pair.Key] = pair.Value;
                                }
                            }
                        }
                    }

                    // TODO: Flag Selected menu item
                    await NavigationHelper.PopulateMenuAsync(shapeFactory, menu, menu, menuItems, viewContext);
                }
            });

        builder.Describe("NavigationItem")
            .OnDisplaying(displaying =>
            {
                var menuItem = displaying.Shape;
                var menu = menuItem.GetProperty<IShape>("Menu");
                var menuName = menu.GetProperty<string>("MenuName");
                var level = menuItem.GetProperty<int>("Level");

                menuItem.Metadata.Alternates.AddRange(NavigationAlternatesFactory.GetNavigationItemAlternates(menuName, level));
            });

        builder.Describe("NavigationItemLink")
            .OnDisplaying(displaying =>
            {
                var menuItem = displaying.Shape;
                var menuName = menuItem.GetProperty<IShape>("Menu").GetProperty<string>("MenuName");
                var level = menuItem.GetProperty<int>("Level");

                // NavigationItemLink__[MenuName] e.g. NavigationItemLink-Main-Menu
                // NavigationItemLink__[MenuName]__level__[level] e.g. NavigationItemLink-Main-Menu-level-2
                menuItem.Metadata.Alternates.AddRange(NavigationAlternatesFactory.GetNavigationItemLinkAlternates(menuName, level));
            });

        return ValueTask.CompletedTask;
    }
}
