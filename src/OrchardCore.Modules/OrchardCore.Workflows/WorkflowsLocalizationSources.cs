using OrchardCore.Localization;

namespace OrchardCore.Workflows;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Workflows.WorkflowsLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class WorkflowsLocalizationSources
{
    /// <summary>Gets the "All Workflow Types" text.</summary>
    public static readonly LocalizationSource AllWorkflowTypes = LocalizationSource.Create("All Workflow Types", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the "Workflows" text.</summary>
    public static readonly LocalizationSource Workflows = LocalizationSource.Create("Workflows", typeof(WorkflowsLocalizationSources));
}
