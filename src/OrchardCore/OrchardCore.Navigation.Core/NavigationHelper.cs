using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
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
        /// <param name="viewContext">The current <see cref="ViewContext"/>.</param>
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
        /// <param name="viewContext">The current <see cref="ViewContext"/>.</param>
        public static async Task PopulateMenuLevelAsync(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems, ViewContext viewContext)
        {
            foreach (MenuItem menuItem in menuItems)
            {
                dynamic menuItemShape = await BuildMenuItemShapeAsync(shapeFactory, parentShape, menu, menuItem, viewContext);

                if (menuItem.Items != null && menuItem.Items.Any())
                {
                    await PopulateMenuLevelAsync(shapeFactory, menuItemShape, menu, menuItem.Items, viewContext);
                }

                await parentShape.AddAsync(menuItemShape, menuItem.Position);
            }
        }

        /// <summary>
        /// Builds a menu item shape.
        /// </summary>
        /// <param name="shapeFactory">The shape factory.</param>
        /// <param name="parentShape">The parent shape.</param>
        /// <param name="menu">The menu shape.</param>
        /// <param name="menuItem">The menu item to build the shape for.</param>
        /// <param name="viewContext">The current <see cref="ViewContext"/>.</param>
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
                .Hash((parentShape.Hash + menuItem.Text.Value).GetHashCode().ToString())
                .Score(0);

            menuItemShape.Id = menuItem.Id;

            if (!String.IsNullOrEmpty(menuItem.Href) && menuItem.Href[0] == '/')
            {
                menuItemShape.Href = QueryHelpers.AddQueryString(menuItem.Href, menu.MenuName, menuItemShape.Hash);
            }

            MarkAsSelectedIfMatchesQueryOrCookie(menuItem, menuItemShape, viewContext);

            foreach (var className in menuItem.Classes)
            {
                menuItemShape.Classes.Add(className);
            }

            return menuItemShape;
        }

        private static void MarkAsSelectedIfMatchesQueryOrCookie(MenuItem menuItem, dynamic menuItemShape, ViewContext viewContext)
        {
            if (!String.IsNullOrEmpty(menuItem.Href) && menuItem.Href[0] == '/')
            {
                var hash = viewContext.HttpContext.Request.Query[(string)menuItemShape.Menu.MenuName];

                if (hash.Count > 0)
                {
                    if (hash[0] == menuItemShape.Hash)
                    {
                        menuItemShape.Score += 2;
                    }
                }
                else
                {
                    var cookie = viewContext.HttpContext.Request.Cookies[menuItemShape.Menu.MenuName + '_' + ShellScope.Context.Settings.Name];

                    if (cookie == menuItemShape.Hash)
                    {
                        menuItemShape.Score++;
                    }
                }
            }

            menuItemShape.Selected = menuItemShape.Score > 0;
        }

        /// <summary>
        /// Ensures only one menuitem (and its ancestors) are marked as selected for the menu.
        /// </summary>
        /// <param name="parentShape">The menu shape.</param>
        /// <param name="viewContext">The current <see cref="ViewContext"/>.</param>
        private static void ApplySelection(dynamic parentShape, ViewContext viewContext)
        {
            var selectedItem = GetHighestPrioritySelectedMenuItem(parentShape);

            // Apply the selection to the hierarchy
            if (selectedItem != null)
            {
                viewContext.HttpContext.Response.Cookies.Append(selectedItem.Menu.MenuName + '_' + ShellScope.Context.Settings.Name, selectedItem.Hash);

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
        /// <returns>The selected menu item shape</returns>
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
