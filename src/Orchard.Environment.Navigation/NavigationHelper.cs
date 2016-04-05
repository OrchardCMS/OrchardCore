using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Navigation
{
    public class NavigationHelper
    {
        /// <summary>
        /// Populates the menu shapes.
        /// </summary>
        /// <param name="shapeFactory">The shape factory.</param>
        /// <param name="parentShape">The menu parent shape.</param>
        /// <param name="menu">The menu shape.</param>
        /// <param name="menuItems">The current level to populate.</param>
        public static void PopulateMenu(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems)
        {
            foreach (MenuItem menuItem in menuItems)
            {
                dynamic menuItemShape = BuildMenuItemShape(shapeFactory, parentShape, menu, menuItem);

                if (menuItem.Items != null && menuItem.Items.Any())
                {
                    PopulateMenu(shapeFactory, menuItemShape, menu, menuItem.Items);
                }

                parentShape.Add(menuItemShape, menuItem.Position);
            }
        }

        /// <summary>
        /// Builds a menu item shape.
        /// </summary>
        /// <param name="shapeFactory">The shape factory.</param>
        /// <param name="parentShape">The parent shape.</param>
        /// <param name="menu">The menu shape.</param>
        /// <param name="menuItem">The menu item to build the shape for.</param>
        /// <returns>The menu item shape.</returns>
        public static dynamic BuildMenuItemShape(dynamic shapeFactory, dynamic parentShape, dynamic menu, MenuItem menuItem)
        {
            var menuItemShape = shapeFactory.MenuItem()
                .Text(menuItem.Text)
                .Id(menuItem.Id)
                .Href(menuItem.Href)
                .LinkToFirstChild(menuItem.LinkToFirstChild)
                .Selected(parentShape.Selected != null && ((bool)parentShape.Selected || IsSelected(menuItem, null)))
                .RouteValues(menuItem.RouteValues)
                .Item(menuItem)
                .Menu(menu)
                .Parent(parentShape)
                .Level(parentShape.Selected == null ? 1 : (int)parentShape.Level + 1)
                .Local(menuItem.LocalNav);

            foreach (var className in menuItem.Classes)
                menuItemShape.Classes.Add(className);

            return menuItemShape;
        }

        public static bool IsSelected(MenuItem menuItem, HttpContext httpContext)
        {
            return false;
        }

        /// <summary>
        /// Determines if a menu item corresponds to a given route.
        /// </summary>
        /// <param name="itemValues">The menu item.</param>
        /// <param name="requestValues">The route data.</param>
        /// <returns>True if the menu item's action corresponds to the route data; false otherwise.</returns>
        public static bool RouteMatches(RouteValueDictionary itemValues, RouteValueDictionary requestValues)
        {
            if (itemValues == null && requestValues == null)
            {
                return true;
            }
            if (itemValues == null || requestValues == null)
            {
                return false;
            }
            if (itemValues.Keys.Any(key => requestValues.ContainsKey(key) == false))
            {
                return false;
            }
            return itemValues.Keys.All(key => string.Equals(Convert.ToString(itemValues[key]), Convert.ToString(requestValues[key]), StringComparison.OrdinalIgnoreCase));
        }
    }
}
