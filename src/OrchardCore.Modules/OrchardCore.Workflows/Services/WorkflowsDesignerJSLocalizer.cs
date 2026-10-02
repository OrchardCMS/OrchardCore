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
            // Shell.
            { "Loading", S["Loading the workflow…"].Value },
            { "LoadFailed", S["The workflow couldn't be loaded. Reload the page to try again."].Value },
            { "History", S["History"].Value },
            { "Undo", S["Undo"].Value },
            { "Redo", S["Redo"].Value },
            { "UndoShortcut", S["Undo (Ctrl+Z)"].Value },
            { "RedoShortcut", S["Redo (Ctrl+Y)"].Value },
            { "Toolbox", S["Toolbox"].Value },
            { "Properties", S["Properties"].Value },
            { "Close", S["Close"].Value },
            { "Cancel", S["Cancel"].Value },

            // Toolbox.
            { "SearchActivities", S["Search activities"].Value },
            { "ActivityKind", S["Kind of activities"].Value },
            { "All", S["All"].Value },
            { "Events", S["Events"].Value },
            { "Tasks", S["Tasks"].Value },
            { "LoadingActivities", S["Loading the activities…"].Value },
            { "LibraryLoadFailed", S["The activities couldn't be loaded."].Value },
            { "NoActivitiesFound", S["No activities match the search."].Value },
            { "AddActivity", S["Add {0}"].Value },
            { "AddActivityFailed", S["The activity couldn't be added. Reload the page and try again."].Value },

            // Properties panel.
            { "ActivityTab", S["Activity"].Value },
            { "WorkflowTab", S["Workflow"].Value },
            { "IssuesTab", S["Issues"].Value },
            { "SelectActivityToEdit", S["Select an activity to edit it."].Value },
            { "MultipleSelected", S["{0} activities are selected."].Value },
            { "MissingActivityCannotBeEdited", S["This activity type isn't available, so it can't be edited. Enable its feature or delete it."].Value },
            { "LoadingForm", S["Loading…"].Value },
            { "FormLoadFailed", S["The form couldn't be loaded."].Value },
            { "Apply", S["Apply"].Value },
            { "Applying", S["Applying…"].Value },
            { "FormHasErrors", S["Fix the errors, then apply."].Value },
            { "ApplyFailed", S["The changes couldn't be applied. Try again."].Value },
            { "DiscardChangesTitle", S["Discard changes?"].Value },
            { "DiscardChangesMessage", S["The activity has errors, so its changes weren't applied. Discard them?"].Value },
            { "Discard", S["Discard"].Value },
            { "KeepEditing", S["Keep editing"].Value },
            { "TransitionsRemoved", S["{0} connection(s) were removed because their outcome no longer exists."].Value },
            { "NoIssues", S["No issues found."].Value },
            { "Error", S["Error"].Value },
            { "Warning", S["Warning"].Value },
            { "CollapsePanel", S["Collapse the panel"].Value },
            { "ExpandPanel", S["Expand the panel"].Value },
            { "ResizePanel", S["Resize the panel"].Value },
            { "ConflictDetected", S["{0} changed this workflow in the meantime. Reload the page to get the latest version."].Value },

            // Canvas.
            { "Canvas", S["Workflow canvas"].Value },
            { "CanvasInstructions", S["Workflow canvas. Tab to an activity to select it; arrow keys move it, Delete removes it, Enter edits it, and the context menu key shows more actions."].Value },
            { "EmptyWorkflow", S["This workflow has no activities yet. Drag an activity from the toolbox to start."].Value },
            { "EmptyWorkflowReadOnly", S["This workflow has no activities."].Value },
            { "Activity", S["Activity"].Value },
            { "Start", S["Start"].Value },
            { "StartActivity", S["start activity"].Value },
            { "IssueCount", S["{0} issue(s)"].Value },
            { "ConnectOutcome", S["Outcome {0}: drag to an activity to connect it, or press Enter to choose one"].Value },
            { "ConnectOutcomeTo", S["Connect an outcome to…"].Value },
            { "ConnectOutcomeOf", S["Connect an outcome of {0}"].Value },
            { "Outcome", S["Outcome"].Value },
            { "Target", S["Target activity"].Value },
            { "Connect", S["Connect"].Value },
            { "ConnectionReplaced", S["The outcome {0} already had a connection; it was replaced."].Value },
            { "RemoveConnection", S["Remove connection {0} → {1}"].Value },
            { "DeleteConnection", S["Delete connection"].Value },
            { "Edit", S["Edit"].Value },
            { "SetAsStart", S["Set as start activity"].Value },
            { "UnsetStart", S["Unset start activity"].Value },
            { "Delete", S["Delete"].Value },
            { "DeleteSelected", S["Delete {0} activities"].Value },
            { "ActivityActions", S["Actions for {0}"].Value },
            { "ConnectionActions", S["Connection actions"].Value },
            { "DeletedActivity", S["Deleted 1 activity."].Value },
            { "DeletedActivities", S["Deleted {0} activities."].Value },
            { "DeletedConnection", S["Deleted the connection."].Value },
            { "Zoom", S["Zoom"].Value },
            { "ZoomIn", S["Zoom in"].Value },
            { "ZoomOut", S["Zoom out"].Value },
            { "ResetZoom", S["Reset zoom to 100%"].Value },
            { "FitToContent", S["Fit to content"].Value },
        };
    }
}
