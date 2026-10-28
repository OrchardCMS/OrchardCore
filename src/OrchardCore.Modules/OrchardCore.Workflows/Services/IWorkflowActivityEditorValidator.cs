using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Checks the settings of an activity the way its editor does when it's saved, so the designer reports an
/// activity whose settings its editor wouldn't accept, like a required value that was never filled in.
/// </summary>
public interface IWorkflowActivityEditorValidator
{
    /// <summary>
    /// Returns the errors the editor of the activity reports for its current settings, or an empty list when the
    /// settings are valid, or when they can't be checked outside of a request or for an activity whose editor
    /// doesn't read them back.
    /// </summary>
    /// <param name="activity">The activity to check. It isn't changed.</param>
    Task<IReadOnlyList<string>> ValidateAsync(ActivityRecord activity);
}
