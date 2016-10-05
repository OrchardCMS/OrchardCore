using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Navigation;
using Orchard.Utility;

namespace Orchard.Navigation
{
    public class MenuShapes : IShapeTableProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MenuShapes(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Menu")
                .OnDisplaying(displaying =>
                {
                    var menu = displaying.Shape;
                    string menuName = menu.MenuName;

                    menu.Classes.Add("menu-" + menuName.HtmlClassify());
                    menu.Classes.Add("menu");
                    menu.Metadata.Alternates.Add("Menu__" + EncodeAlternateElement(menuName));
                })
                .OnProcessing(processing =>
                {
                    dynamic menu = processing.Shape;
                    string menuName = menu.MenuName;

                    // Menu population is executed when processing the shape so that its value
                    // can be cached. IShapeDisplayEvents is called before the ShapeDescriptor
                    // events and thus this code can be cached.

                    if ((bool)menu.HasItems)
                    {
                        return;
                    }

                    var httpContext = _httpContextAccessor.HttpContext;
                    var navigationManager = httpContext.RequestServices.GetService<INavigationManager>();
                    var shapeFactory = httpContext.RequestServices.GetService<IShapeFactory>();

                    IEnumerable<MenuItem> menuItems = navigationManager.BuildMenu(menuName, processing.DisplayContext.ViewContext);

                    // adding query string parameters
                    RouteData route = menu.RouteData;
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

                    // TODO: Flag Selected menu item

                    NavigationHelper.PopulateMenu(shapeFactory, menu, menu, menuItems);

                });

            builder.Describe("MenuItem")
                .OnDisplaying(displaying =>
                {
                    var menuItem = displaying.Shape;
                    var menu = menuItem.Menu;
                    var menuName = menu.MenuName;
                    int level = menuItem.Level;

                    menuItem.Metadata.Alternates.Add("MenuItem__level__" + level);
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menuName));
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menuName) + "__level__" + level);
                });

            builder.Describe("MenuItemLink")
                .OnDisplaying(displaying =>
                {
                    var menuItem = displaying.Shape;
                    string menuName = menuItem.Menu.MenuName;
                    string contentType = null;
                    int level = menuItem.Level;

                    menuItem.Metadata.Alternates.Add("MenuItemLink__level__" + level);

                    // MenuItemLink__[ContentType] e.g. MenuItemLink-HtmlMenuItem
                    // MenuItemLink__[ContentType]__level__[level] e.g. MenuItemLink-HtmlMenuItem-level-2
                    if (contentType != null)
                    {
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(contentType));
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(contentType) + "__level__" + level);
                    }

                    // MenuItemLink__[MenuName] e.g. MenuItemLink-Main-Menu
                    // MenuItemLink__[MenuName]__level__[level] e.g. MenuItemLink-Main-Menu-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName));
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__level__" + level);

                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem
                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem-level-2
                    if (contentType != null)
                    {
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType));
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType) + "__level__" + level);
                    }
                });
        }

        /// <summary>
        /// Encodes dashed and dots so that they don't conflict in filenames
        /// </summary>
        /// <param name="alternateElement"></param>
        /// <returns></returns>
        private string EncodeAlternateElement(string alternateElement)
        {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }
    }
}