using OrchardCore.DisplayManagement;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// A server-rendered form returned to the designer as <c>{ valid, content, scripts, styles }</c>: either an
/// activity editor shape, or a partial view.
/// </summary>
public sealed class WorkflowDesignerFragmentViewModel
{
    /// <summary>
    /// The shape to render, for activity editors.
    /// </summary>
    public IShape Shape { get; init; }

    /// <summary>
    /// The partial view to render when <see cref="Shape"/> is not set.
    /// </summary>
    public string PartialName { get; init; }

    /// <summary>
    /// The model of <see cref="PartialName"/>.
    /// </summary>
    public object PartialModel { get; init; }

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
