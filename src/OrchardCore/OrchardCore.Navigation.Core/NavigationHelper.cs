using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Environment.Shell.Scope;

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
            ApplySelection(parentShape, viewContext);
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
                .Priority(menuItem.Priority)
                .Local(menuItem.LocalNav)
                .Score(0);

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
            if (RouteMatches(menuItem.RouteValues, viewContext.RouteData.Values))
            {
                menuItemShape.Score += 2;
            }

            // if route match failed, try comparing URL strings
            else if (!String.IsNullOrWhiteSpace(menuItem.Href) && menuItem.Href[0] == '/')
            {
                PathString path = menuItem.Href;

                if (viewContext.HttpContext.Request.PathBase.HasValue)
                {
                    if (path.StartsWithSegments(viewContext.HttpContext.Request.PathBase, StringComparison.OrdinalIgnoreCase, out var remaining))
                    {
                        path = remaining;
                    }
                }

                if (viewContext.HttpContext.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    menuItemShape.Score += 2;
                }
            }

            var cookie = viewContext.HttpContext.Request.Cookies["selectedMenuItem_" + ShellScope.Context.Settings.Name];
            if (cookie == menuItemShape.Parent?.Text?.Value + menuItem.Text.Value + menuItem.Href)
            {
                menuItemShape.Score += 1;
            }

            menuItemShape.Selected = menuItemShape.Score > 0;
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

            foreach (var key in itemValues.Keys)
            {
                if (!String.Equals(itemValues[key]?.ToString(), requestValues[key]?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Ensures only one menuitem (and its ancestors) are marked as selected for the menu.
        /// </summary>
        /// <param name="parentShape">The menu shape.</param>    
        private static void ApplySelection(dynamic parentShape, ViewContext viewContext)
        {
            var selectedItem = GetHighestPrioritySelectedMenuItem(parentShape);

            // Apply the selection to the hierarchy
            if (selectedItem != null)
            {
                viewContext.HttpContext.Response.Cookies.Append("selectedMenuItem_" + ShellScope.Context.Settings.Name, selectedItem.Parent?.Text?.Value + selectedItem.Text.Value + selectedItem.Href);
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
                        if (item.Score > result.Score)
                        {
                            result.Selected = false;
                            result = item;
                        }
                        else if (item.Priority > result.Priority)
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
