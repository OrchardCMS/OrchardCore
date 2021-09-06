using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Navigation
{
    public class NavigationShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Navigation")
                .OnDisplaying(displaying =>
                {
                    var menu = displaying.Shape;
                    var menuName = menu.GetProperty<string>("MenuName");

                    menu.Classes.Add("menu-" + menuName.HtmlClassify());
                    menu.Classes.Add("menu");
                    menu.Metadata.Alternates.Add("Navigation__" + EncodeAlternateElement(menuName));
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

                    var navigationManager = context.ServiceProvider.GetRequiredService<INavigationManager>();
                    var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                    var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
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
                });

            builder.Describe("NavigationItem")
                .OnDisplaying(displaying =>
                {
                    var menuItem = displaying.Shape;
                    var menu = menuItem.GetProperty<IShape>("Menu");
                    var menuName = menu.GetProperty<string>("MenuName");
                    var level = menuItem.GetProperty<int>("Level");

                    menuItem.Metadata.Alternates.Add("NavigationItem__level__" + level);
                    menuItem.Metadata.Alternates.Add("NavigationItem__" + EncodeAlternateElement(menuName));
                    menuItem.Metadata.Alternates.Add("NavigationItem__" + EncodeAlternateElement(menuName) + "__level__" + level);
                });

            builder.Describe("NavigationItemLink")
                .OnDisplaying(displaying =>
                {
                    var menuItem = displaying.Shape;
                    var menuName = menuItem.GetProperty<IShape>("Menu").GetProperty<string>("MenuName");
                    var level = menuItem.GetProperty<int>("Level");

                    menuItem.Metadata.Alternates.Add("NavigationItemLink__level__" + level);

                    // NavigationItemLink__[MenuName] e.g. NavigationItemLink-Main-Menu
                    // NavigationItemLink__[MenuName]__level__[level] e.g. NavigationItemLink-Main-Menu-level-2
                    menuItem.Metadata.Alternates.Add("NavigationItemLink__" + EncodeAlternateElement(menuName));
                    menuItem.Metadata.Alternates.Add("NavigationItemLink__" + EncodeAlternateElement(menuName) + "__level__" + level);
                });
        }

        /// <summary>
        /// Encodes dashed and dots so that they don't conflict in filenames
        /// </summary>
        /// <param name="alternateElement"></param>
        /// <returns></returns>
        private static string EncodeAlternateElement(string alternateElement)
        {
            return alternateElement.Replace("-", "__").Replace('.', '_');
        }
    }
}
