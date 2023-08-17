using System;
using System.Collections.Generic;

namespace OrchardCore.AdminMenu.Models
{
    public class AdminMenu
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public List<AdminNode> MenuItems { get; } = new List<AdminNode>();

        public AdminNode GetMenuItemById(string id)
        {
            foreach (var menuItem in MenuItems)
            {
                var found = menuItem.GetMenuItemById(id);
                if (found != null)
                {
                    return found;
                }
            }

            // not found
            return null;
        }

        public bool RemoveMenuItem(AdminNode itemToRemove)
        {
            if (MenuItems.Contains(itemToRemove)) // todo: avoid this check by having a single TreeNode as a property of the content tree preset.
            {
                MenuItems.Remove(itemToRemove);
                return true; // success
            }
            else
            {
                foreach (var firstLevelMenuItem in MenuItems)
                {
                    if (firstLevelMenuItem.RemoveMenuItem(itemToRemove))
                    {
                        return true; // success
                    }
                }
            }

            return false; // failure
        }

        public bool InsertMenuItemAt(AdminNode menuItemToInsert, AdminNode destinationMenuItem, int position)
        {
            if (menuItemToInsert == null)
            {
                throw new ArgumentNullException(nameof(menuItemToInsert));
            }

            // insert the node at the destination node
            if (destinationMenuItem == null)
            {
                MenuItems.Insert(position, menuItemToInsert);
                return true; // success
            }
            else
            {
                foreach (var firstLevelMenuItem in MenuItems)
                {
                    if (firstLevelMenuItem.InsertMenuItem(menuItemToInsert, destinationMenuItem, position))
                    {
                        return true; // success
                    }
                }
            }
            return false; // failure
        }
    }
}
