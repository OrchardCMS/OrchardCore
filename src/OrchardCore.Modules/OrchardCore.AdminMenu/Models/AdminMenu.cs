using System;
using System.Collections.Immutable;

namespace OrchardCore.AdminMenu.Models
{
    public class AdminMenu
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public ImmutableArray<AdminNode> MenuItems { get; set; } = ImmutableArray.Create<AdminNode>();

        public AdminNode GetMenuItemById(string id)
        {
            foreach (var menuItem in MenuItems)
            {
                var found = menuItem.GetMenuItemById(id);
                if (found != null)
                {
                    return CloneMenuItem(found);
                }
            }

            // not found
            return null;
        }

        public bool RemoveMenuItem(AdminNode itemToRemove)
        {
            if (MenuItems.Contains(itemToRemove)) // todo: avoid this check by having a single TreeNode as a property of the content tree preset.
            {
                MenuItems = MenuItems.Remove(itemToRemove);
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
                throw new ArgumentNullException("menuItemToInsert");
            }

            // insert the node at the destination node
            if (destinationMenuItem == null)
            {
                MenuItems = MenuItems.Insert(position, menuItemToInsert);
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

        /// <summary>
        /// Creates a new parent list referencing a shallow copy of this node.
        /// </summary>
        public AdminNode CloneMenuItem(AdminNode nodeToClone)
        {
            if (MenuItems.Contains(nodeToClone))
            {
                var clone = nodeToClone.Clone();
                MenuItems = MenuItems.Replace(nodeToClone, clone);
                return clone;
            }
            else
            {
                foreach (var firstLevelMenuItem in MenuItems)
                {
                    var clone = firstLevelMenuItem.Clone(nodeToClone);
                    if (clone != null)
                    {
                        return clone;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a shallow copy of this menu.
        /// </summary>
        public virtual AdminMenu Clone()
        {
            return new AdminMenu()
            {
                Id = Id,
                Name = Name,
                Enabled = Enabled,
                MenuItems = MenuItems
            };
        }
    }
}
