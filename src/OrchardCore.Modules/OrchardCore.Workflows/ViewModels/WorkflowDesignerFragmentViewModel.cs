using OrchardCore.DisplayManagement;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// A server-rendered form returned to the designer as <c>{ valid, content, scripts, styles }</c>: the editor of an
/// activity, or of the properties of the workflow type.
/// </summary>
public sealed class WorkflowDesignerFragmentViewModel
{
    /// <summary>
    /// The editor shape to render.
    /// </summary>
    public IShape Shape { get; init; }

    /// <summary>
    /// The document identifier of the workflow type, exposed as <c>data-workflow-type-id</c>.
    /// </summary>
    public long WorkflowTypeId { get; init; }

    /// <summary>
    /// The activity being edited, exposed as <c>data-activity-id</c>.
    /// </summary>
    public string ActivityId { get; init; }

    /// <summary>
    /// Whether the posted form was valid; <see langword="false"/> when it is rendered again with errors.
    /// </summary>
    public bool Valid { get; init; } = true;
}
