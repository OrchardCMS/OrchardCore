using System;
using System.Collections.Generic;
using OrchardCore.Navigation;


namespace OrchardCore.Sitemaps.Models
{
    public class SitemapMenu
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public List<SitemapNode> MenuItems { get; } = new List<SitemapNode>();


        public SitemapNode GetMenuItemById(string id)
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

        public bool RemoveMenuItem(SitemapNode itemToRemove)
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

        public bool InsertMenuItemAt(SitemapNode menuItemToInsert, SitemapNode destinationMenuItem, int position)
        {
            if (menuItemToInsert == null)
            {
                throw new ArgumentNullException("menuItemToInsert");
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
