using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Routing;

namespace OrchardCore.Navigation
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
        public static async Task PopulateMenuAsync(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems, ViewContext viewContext)
        {
            await PopulateMenuLevelAsync(shapeFactory, parentShape, menu, menuItems, viewContext);
            ApplySelection(parentShape);
        }

        /// <summary>
        /// Populates the menu shapes for the level recursively.
        /// </summary>
        /// <param name="shapeFactory">The shape factory.</param>
        /// <param name="parentShape">The menu parent shape.</param>
        /// <param name="menu">The menu shape.</param>
        /// <param name="menuItems">The current level to populate.</param>
        public static async Task PopulateMenuLevelAsync(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems, ViewContext viewContext)
        {
            foreach (MenuItem menuItem in menuItems)
            {
                dynamic menuItemShape = await BuildMenuItemShapeAsync(shapeFactory, parentShape, menu, menuItem, viewContext);

                if (menuItem.Items != null && menuItem.Items.Any())
                {
                    await PopulateMenuLevelAsync(shapeFactory, menuItemShape, menu, menuItem.Items, viewContext);
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
        private static async Task<dynamic> BuildMenuItemShapeAsync(dynamic shapeFactory, dynamic parentShape, dynamic menu, MenuItem menuItem, ViewContext viewContext)
        {
            var menuItemShape = (await shapeFactory.NavigationItem())
                .Text(menuItem.Text)
                .Href(menuItem.Href)
                .Url(menuItem.Url)
                .LinkToFirstChild(menuItem.LinkToFirstChild)
                .RouteValues(menuItem.RouteValues)
                .Item(menuItem)
                .Menu(menu)
                .Parent(parentShape)
                .Level(parentShape.Level == null ? 1 : (int)parentShape.Level + 1)
                .SelectionPriority(menuItem.Priority)
                .Local(menuItem.LocalNav);

            menuItemShape.Id = menuItem.Id;

            MarkAsSelectedIfMatchesRouteOrUrl(menuItem, menuItemShape, viewContext);

            foreach (var className in menuItem.Classes)
            {
                menuItemShape.Classes.Add(className);
            }

            return menuItemShape;
        }

        private static void MarkAsSelectedIfMatchesRouteOrUrl(MenuItem menuItem, dynamic menuItemShape, ViewContext viewContext)
        {
            // compare route values (if any) first
            bool match = menuItem.RouteValues != null && RouteMatches(menuItem.RouteValues, viewContext.RouteData.Values);

            // if route match failed, try comparing URL strings, if
            if (!match && !String.IsNullOrWhiteSpace(menuItem.Href) && menuItem.Href[0] == '/')
            {
                PathString pathString = menuItem.Href;

                if (viewContext.HttpContext.Request.PathBase.HasValue)
                {
                    if (pathString.StartsWithNormalizedSegments(viewContext.HttpContext.Request.PathBase, StringComparison.OrdinalIgnoreCase, out var remaining))
                    {
                        pathString = remaining;
                    }
                }

                match = viewContext.HttpContext.Request.Path.StartsWithNormalizedSegments(menuItem.Href, StringComparison.OrdinalIgnoreCase);
            }

            menuItemShape.Selected = match;
        }

        /// <summary>
        /// Determines if a menu item corresponds to a given route.
        /// </summary>
        /// <param name="itemValues">The menu item.</param>
        /// <param name="requestValues">The route data.</param>
        /// <returns>True if the menu item's action corresponds to the route data; false otherwise.</returns>
        private static bool RouteMatches(RouteValueDictionary itemValues, RouteValueDictionary requestValues)
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


        /// <summary>
        /// Ensures only one menuitem (and its ancestors) are marked as selected for the menu.
        /// </summary>
        /// <param name="parentShape">The menu shape.</param>    
        private static void ApplySelection(dynamic parentShape)
        {
            var selectedItem = GetHighestPrioritySelectedMenuItem(parentShape);

            // Apply the selection to the hierarchy
            if (selectedItem != null)
            {
                while (selectedItem.Parent != null)
                {
                    selectedItem = selectedItem.Parent;
                    selectedItem.Selected = true;
                }
            }
        }


        /// <summary>
        /// Traverses the menu and returns the selected item with the highest priority
        /// </summary>
        /// <param name="parentShape">The menu shape.</param>
        /// /// <returns>The selected menu item shape</returns>
        private static dynamic GetHighestPrioritySelectedMenuItem(dynamic parentShape)
        {
            dynamic result = null;

            var tempStack = new Stack<dynamic>(new dynamic[] { parentShape });

            while (tempStack.Any())
            {
                // evaluate first
                dynamic item = tempStack.Pop();


                if (item.Selected == true)
                {
                    if (result == null) // found the first one
                    {
                        result = item;
                    }
                    else // found more selected: tie break required.
                    {
                        if (item.Priority > result.Priority)
                        {
                            result.Selected = false;
                            result = item;
                        }
                        else
                        {
                            item.Selected = false;
                        }
                    }
                }

                // add children to the stack to be evaluated too
                foreach (var i in item.Items)
                {
                    tempStack.Push(i);
                }
            }

            return result;
        }
    }
}
