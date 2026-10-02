namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// The activities the designer toolbox offers, grouped by category.
/// </summary>
public sealed class WorkflowDesignerLibrary
{
    /// <summary>
    /// The categories, in display order.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerCategory> Categories { get; init; } = [];
}

/// <summary>
/// An activity category of the toolbox.
/// </summary>
public sealed class WorkflowDesignerCategory
{
    /// <summary>
    /// The localized category name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The activities of the category.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerActivityDescriptor> Activities { get; init; } = [];
}

/// <summary>
/// An activity type of the toolbox.
/// </summary>
public sealed class WorkflowDesignerActivityDescriptor
{
    /// <summary>
    /// The activity type name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The localized display text.
    /// </summary>
    public string DisplayText { get; init; }

    /// <summary>
    /// The localized category.
    /// </summary>
    public string Category { get; init; }

    /// <summary>
    /// Whether the activity is an event.
    /// </summary>
    public bool IsEvent { get; init; }

    /// <summary>
    /// Whether the activity has an editor of its own.
    /// </summary>
    public bool HasEditor { get; init; }

    /// <summary>
    /// The rendered <c>{Name}_Fields_Thumbnail</c> shape.
    /// </summary>
    public string ThumbnailHtml { get; init; }

    /// <summary>
    /// The Font Awesome icon class: the one set on the activity's registration, else a default for its category.
    /// </summary>
    public string Icon { get; init; }
}
