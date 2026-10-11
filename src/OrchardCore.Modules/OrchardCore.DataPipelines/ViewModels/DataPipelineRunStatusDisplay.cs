using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.ViewModels;

/// <summary>
/// How the admin pages show the status of a run.
/// </summary>
public static class DataPipelineRunStatusDisplay
{
    /// <summary>
    /// Gets the classes of the icon of a status.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>The Font Awesome classes, with a color.</returns>
    public static string GetIcon(this DataPipelineRunStatus status)
        => status switch
        {
            DataPipelineRunStatus.Succeeded => "fa-solid fa-circle-check text-success",
            DataPipelineRunStatus.Failed => "fa-solid fa-circle-xmark text-danger",
            DataPipelineRunStatus.Cancelled => "fa-solid fa-ban text-secondary",
            DataPipelineRunStatus.Running => "fa-solid fa-spinner text-primary",
            _ => "fa-regular fa-clock text-secondary",
        };
}
