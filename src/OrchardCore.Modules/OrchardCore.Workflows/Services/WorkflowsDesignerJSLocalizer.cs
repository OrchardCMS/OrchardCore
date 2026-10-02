using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// The client strings of the workflow designer (group <c>workflows-designer</c>).
/// </summary>
public sealed class WorkflowsDesignerJSLocalizer : IJSLocalizer
{
    /// <summary>
    /// The localization group the designer requests.
    /// </summary>
    public const string Group = "workflows-designer";

    internal readonly IStringLocalizer S;

    public WorkflowsDesignerJSLocalizer(IStringLocalizer<WorkflowsDesignerJSLocalizer> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (!string.Equals(group, Group, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new Dictionary<string, string>
        {
            { "Loading", S["Loading the workflow…"].Value },
            { "LoadFailed", S["The workflow couldn't be loaded. Reload the page to try again."].Value },
            { "History", S["History"].Value },
            { "Undo", S["Undo"].Value },
            { "Redo", S["Redo"].Value },
            { "Toolbox", S["Toolbox"].Value },
            { "Canvas", S["Workflow canvas"].Value },
            { "Properties", S["Properties"].Value },
        };
    }
}
