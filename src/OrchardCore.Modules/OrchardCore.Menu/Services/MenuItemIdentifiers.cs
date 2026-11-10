using System.Text.Json.Nodes;

namespace OrchardCore.Menu.Services;

/// <summary>
/// Gives a valid identifier to the menu items that have none. The menu admin finds a menu item by its
/// <c>ContentItemId</c>, so a menu item without a usable one, for example from a recipe that omits it or stores a
/// value that is not an identifier, could not be edited or deleted.
/// </summary>
internal static class MenuItemIdentifiers
{
    /// <summary>
    /// Assigns a new identifier to every menu item of the content, including nested menu items, whose
    /// <c>ContentItemId</c> is missing, is not an identifier, or repeats the identifier of another menu item.
    /// </summary>
    /// <param name="content">The <c>Content</c> of a menu.</param>
    /// <param name="generateId">Creates a new content item identifier.</param>
    /// <returns><see langword="true"/> when at least one menu item received an identifier.</returns>
    public static bool AssignMissing(JsonObject content, Func<string> generateId)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(generateId);

        return AssignMissing(content, generateId, new HashSet<string>(StringComparer.Ordinal));
    }

    /// <summary>
    /// Returns the <c>ContentItemId</c> of a menu item when it is a usable identifier: a non-empty string of letters,
    /// digits, '-' and '_'.
    /// </summary>
    public static string GetValidId(JsonObject menuItem)
    {
        if (menuItem["ContentItemId"] is not JsonValue value || !value.TryGetValue<string>(out var id) || string.IsNullOrEmpty(id))
        {
            return null;
        }

        foreach (var character in id)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character != '-' && character != '_')
            {
                return null;
            }
        }

        return id;
    }

    private static bool AssignMissing(JsonObject content, Func<string> generateId, HashSet<string> usedIds)
    {
        if (content["MenuItemsListPart"]?["MenuItems"] is not JsonArray menuItems)
        {
            return false;
        }

        var changed = false;

        foreach (var node in menuItems)
        {
            if (node is not JsonObject menuItem)
            {
                continue;
            }

            var id = GetValidId(menuItem);

            if (id is null || !usedIds.Add(id))
            {
                id = generateId();
                usedIds.Add(id);
                menuItem["ContentItemId"] = id;
                changed = true;
            }

            changed |= AssignMissing(menuItem, generateId, usedIds);
        }

        return changed;
    }
}
