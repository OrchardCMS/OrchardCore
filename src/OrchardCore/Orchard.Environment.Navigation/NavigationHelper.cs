using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

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
        public static void PopulateMenu(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems, ViewContext viewContext)
        {
            foreach (MenuItem menuItem in menuItems)
            {
                dynamic menuItemShape = BuildMenuItemShape(shapeFactory, parentShape, menu, menuItem, viewContext);

                if (menuItem.Items != null && menuItem.Items.Any())
                {
                    PopulateMenu(shapeFactory, menuItemShape, menu, menuItem.Items, viewContext);
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
        public static dynamic BuildMenuItemShape(dynamic shapeFactory, dynamic parentShape, dynamic menu, MenuItem menuItem, ViewContext viewContext)
        {
            var menuItemShape = shapeFactory.NavigationItem()
                .Text(menuItem.Text)
                .Href(menuItem.Href)
                .LinkToFirstChild(menuItem.LinkToFirstChild)
                .RouteValues(menuItem.RouteValues)
                .Item(menuItem)
                .Menu(menu)
                .Parent(parentShape)
                .Level(parentShape.Level == null ? 1 : (int)parentShape.Level + 1)
                .Local(menuItem.LocalNav);

			menuItemShape.Id = menuItem.Id;

			ApplySelection(menuItem, menuItemShape, viewContext);

            foreach (var className in menuItem.Classes)
                menuItemShape.Classes.Add(className);

            return menuItemShape;
        }

        public static void ApplySelection(MenuItem menuItem, dynamic menuItemShape, ViewContext viewContext)
        {
            // compare route values (if any) first
            bool match = menuItem.RouteValues != null && RouteMatches(menuItem.RouteValues, viewContext.RouteData.Values);

            // if route match failed, try comparing URL strings, if
            if (!match && menuItem.Href != null)
            {
                string url = menuItem.Href.Replace("~/", viewContext.HttpContext.Request.PathBase);
                match = viewContext.HttpContext.Request.Path.Equals(url, StringComparison.OrdinalIgnoreCase);
            }

            menuItemShape.Selected = match;

            // Apply the selection to the hierarchy
            if (match)
            {
                while (menuItemShape.Parent != null)
                {
                    menuItemShape = menuItemShape.Parent;
                    menuItemShape.Selected = true;
                }
            }
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
