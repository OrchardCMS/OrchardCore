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
                .IdHint(menuItem.IdHint)
                .Href(menuItem.Href)
                .LinkToFirstChild(menuItem.LinkToFirstChild)
                .Selected(menuItem.Selected)
                .RouteValues(menuItem.RouteValues)
                .Item(menuItem)
                .Menu(menu)
                .Parent(parentShape)
                .Level(menuItem.Level);

            foreach (var className in menuItem.Classes)
                menuItemShape.Classes.Add(className);

            return menuItemShape;
        }
    }
}
