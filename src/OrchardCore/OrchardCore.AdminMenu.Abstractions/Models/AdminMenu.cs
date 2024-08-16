namespace OrchardCore.AdminMenu.Models;

public class AdminMenu
{
    public string Id { get; set; } = Guid.NewGuid().ToString("n");
    public string Name { get; set; }
    public bool Enabled { get; set; } = true;
    public List<AdminNode> MenuItems { get; init; } = [];

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
        if (MenuItems.Remove(itemToRemove)) // todo: avoid this check by having a single TreeNode as a property of the content tree preset.
        {
            return true; // success
        }

        foreach (var firstLevelMenuItem in MenuItems)
        {
            if (firstLevelMenuItem.RemoveMenuItem(itemToRemove))
            {
                return true; // success
            }
        }

        return false; // failure
    }

    public bool InsertMenuItemAt(AdminNode menuItemToInsert, AdminNode destinationMenuItem, int position)
    {
        ArgumentNullException.ThrowIfNull(menuItemToInsert);

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
