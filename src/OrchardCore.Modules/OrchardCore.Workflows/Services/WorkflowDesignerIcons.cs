using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Resolves the icon the designer shows for an activity: the one set on its <see cref="ActivityRegistration"/>,
/// else a default for its category, else a generic event or task icon.
/// </summary>
internal static class WorkflowDesignerIcons
{
    public const string Missing = "fa-solid fa-circle-exclamation";
    public const string Event = "fa-solid fa-bolt";
    public const string Task = "fa-solid fa-gear";

    // Keyed by the category's resource name (LocalizedString.Name), which doesn't change with the culture.
    private static readonly Dictionary<string, string> s_categoryIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Background"] = "fa-solid fa-clock",
        ["Content"] = "fa-solid fa-file-lines",
        ["Control Flow"] = "fa-solid fa-diagram-project",
        ["Exceptions"] = "fa-solid fa-bug",
        ["HTTP"] = "fa-solid fa-globe",
        ["Messaging"] = "fa-solid fa-envelope",
        ["Meta"] = "fa-solid fa-tag",
        ["Notifications"] = "fa-solid fa-bell",
        ["Primitives"] = "fa-solid fa-cube",
        ["Session"] = "fa-solid fa-database",
        ["Social"] = "fa-solid fa-share-nodes",
        ["Tenant"] = "fa-solid fa-building",
        ["UI"] = "fa-solid fa-window-maximize",
        ["User"] = "fa-solid fa-user",
        ["Validation"] = "fa-solid fa-check-double",
    };

    public static string Resolve(IActivity activity, WorkflowOptions options)
    {
        if (activity is MissingActivity)
        {
            return Missing;
        }

        var icon = options.GetActivityRegistration(activity.GetType())?.Icon;

        if (!string.IsNullOrWhiteSpace(icon))
        {
            return icon;
        }

        var category = activity.Category?.Name;

        if (category is not null && s_categoryIcons.TryGetValue(category, out var categoryIcon))
        {
            return categoryIcon;
        }

        return activity.IsEvent() ? Event : Task;
    }
}
