using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Resolves the icon the designer shows for an activity: the one set on its <see cref="ActivityRegistration"/>,
/// else the one its module set for its category (<see cref="WorkflowOptions.CategoryIcons"/>), else a generic event
/// or task icon.
/// </summary>
internal static class WorkflowDesignerIcons
{
    public const string Missing = "fa-solid fa-circle-exclamation";
    public const string Event = "fa-solid fa-bolt";
    public const string Task = "fa-solid fa-gear";

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

        if (category is not null && options.CategoryIcons.TryGetValue(category, out var categoryIcon) && !string.IsNullOrWhiteSpace(categoryIcon))
        {
            return categoryIcon;
        }

        return activity.IsEvent() ? Event : Task;
    }
}
