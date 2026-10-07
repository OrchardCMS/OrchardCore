using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// The graph the designer edits: the draft when there is one, otherwise the live workflow type.
/// </summary>
public sealed class WorkflowDesignerDefinition
{
    /// <summary>
    /// The document identifier of the workflow type.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// The <see cref="WorkflowType.WorkflowTypeId"/>.
    /// </summary>
    public string WorkflowTypeId { get; init; }

    /// <summary>
    /// The draft revision, or 0 when there is no draft.
    /// </summary>
    public int Revision { get; init; }

    /// <summary>
    /// Whether the graph comes from a draft.
    /// </summary>
    public bool HasDraft { get; init; }

    /// <summary>
    /// The name of the user who last changed the draft.
    /// </summary>
    public string DraftModifiedBy { get; init; }

    /// <summary>
    /// The identifier of the user who last changed the draft.
    /// </summary>
    public string DraftModifiedByUserId { get; init; }

    /// <summary>
    /// When the draft was last changed.
    /// </summary>
    public DateTime? DraftModifiedUtc { get; init; }

    /// <summary>
    /// The workflow type properties.
    /// </summary>
    public WorkflowTypeDraftSettings Settings { get; init; }

    /// <summary>
    /// The activities.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerNode> Nodes { get; init; } = [];

    /// <summary>
    /// The transitions.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerTransition> Transitions { get; init; } = [];

    /// <summary>
    /// The validation issues.
    /// </summary>
    public IReadOnlyList<WorkflowDesignIssue> Issues { get; init; } = [];

    /// <summary>
    /// The number of instances of this type that haven't completed.
    /// </summary>
    public int RunningInstanceCount { get; init; }

    /// <summary>
    /// The instance shown by the read-only instance viewer, or <see langword="null"/> in the designer.
    /// </summary>
    public WorkflowDesignerInstance Instance { get; init; }

    /// <summary>
    /// The variables the definition declares.
    /// </summary>
    public IReadOnlyList<WorkflowVariableDefinition> Variables { get; init; } = [];

    /// <summary>
    /// The variable types available for declarations.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerVariableType> VariableTypes { get; init; } = [];

    /// <summary>
    /// The global values and functions every expression can use.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerGlobalValue> GlobalValues { get; init; } = [];

    /// <summary>
    /// The version new instances start on, or <see langword="null"/> when the workflow type has none.
    /// </summary>
    public WorkflowDesignerVersion PublishedVersion { get; init; }

    /// <summary>
    /// The version shown: the one a read-only version page shows, or the one the instance of the instance
    /// viewer runs on. <see langword="null"/> in the designer, and for instances created before versions existed.
    /// </summary>
    public WorkflowDesignerVersion Version { get; init; }
}

/// <summary>
/// An activity as shown on the designer canvas.
/// </summary>
public sealed class WorkflowDesignerNode
{
    /// <summary>
    /// The <see cref="ActivityRecord.ActivityId"/>.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// The activity type name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The left coordinate, in canvas pixels at 100% zoom.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    /// The top coordinate, in canvas pixels at 100% zoom.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    /// Whether the activity is a start activity.
    /// </summary>
    public bool IsStart { get; init; }

    /// <summary>
    /// Whether the activity is an event.
    /// </summary>
    public bool IsEvent { get; init; }

    /// <summary>
    /// Whether the activity has an editor of its own.
    /// </summary>
    public bool HasEditor { get; init; }

    /// <summary>
    /// Whether the activity type isn't registered any more.
    /// </summary>
    public bool IsMissing { get; init; }

    /// <summary>
    /// The custom title of the activity, or its display text.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// The display text of the activity type.
    /// </summary>
    public string DisplayText { get; init; }

    /// <summary>
    /// The category of the activity type.
    /// </summary>
    public string Category { get; init; }

    /// <summary>
    /// The rendered <c>{Name}_Fields_Design</c> shape.
    /// </summary>
    public string DesignHtml { get; init; }

    /// <summary>
    /// The Font Awesome icon class of the activity.
    /// </summary>
    public string Icon { get; init; }

    /// <summary>
    /// The outcomes the activity can produce, given its current properties.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerOutcome> Outcomes { get; init; } = [];

    /// <summary>
    /// The outputs the activity declares (see <see cref="Activities.IActivityOutputs"/>).
    /// </summary>
    public IReadOnlyList<WorkflowDesignerOutput> Outputs { get; init; } = [];

    /// <summary>
    /// The variable each output is bound to, by output name.
    /// </summary>
    public IReadOnlyDictionary<string, string> OutputBindings { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// The values the activity provides to the activities after it (see <see cref="Activities.IActivityProvidedValues"/>).
    /// </summary>
    public IReadOnlyList<WorkflowDesignerProvidedValue> ProvidedValues { get; init; } = [];
}

/// <summary>
/// An output of an activity, as shown by the designer.
/// </summary>
public sealed class WorkflowDesignerOutput
{
    /// <summary>
    /// The name of the output.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The variable type of its values.
    /// </summary>
    public string TypeName { get; init; }

    /// <summary>
    /// The name shown in the designer.
    /// </summary>
    public string DisplayName { get; init; }
}

/// <summary>
/// A value an activity provides, as shown by the designer.
/// </summary>
public sealed class WorkflowDesignerProvidedValue
{
    /// <summary>
    /// <c>Input</c>, <c>Output</c> or <c>Properties</c>.
    /// </summary>
    public string Source { get; init; }

    /// <summary>
    /// The key of the value in its source.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The variable type name of the value.
    /// </summary>
    public string TypeName { get; init; }

    /// <summary>
    /// What the value is.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// The fields of the value; their <see cref="Source"/> is <see langword="null"/>.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerProvidedValue> Members { get; init; } = [];

    /// <summary>
    /// Whether the expressions of the activity that provides the value can read it too.
    /// </summary>
    public bool AvailableToItself { get; init; }
}

/// <summary>
/// A global value or function, as listed by the designer (<see cref="Services.WorkflowGlobalValue"/>).
/// </summary>
public sealed class WorkflowDesignerGlobalValue
{
    /// <summary>
    /// <c>Value</c> or <c>Function</c>.
    /// </summary>
    public string Kind { get; init; }

    /// <summary>
    /// The name shown in the designer.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The variable type name of the value, or of what the function returns.
    /// </summary>
    public string TypeName { get; init; }

    /// <summary>
    /// What it is.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// The Liquid path that reads the value, or <see langword="null"/>.
    /// </summary>
    public string LiquidPath { get; init; }

    /// <summary>
    /// The JavaScript that reads the value or calls the function, or <see langword="null"/>.
    /// </summary>
    public string JavaScript { get; init; }

    /// <summary>
    /// The fields of the value; their <see cref="WorkflowDesignerProvidedValue.Source"/> is <see langword="null"/>.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerProvidedValue> Members { get; init; } = [];
}

/// <summary>
/// A variable type, as offered by the designer.
/// </summary>
public sealed class WorkflowDesignerVariableType
{
    /// <summary>
    /// The <see cref="Services.IWorkflowVariableType.Name"/>.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The name shown in the designer.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// How a default value of this type is edited.
    /// </summary>
    public string Editor { get; init; }
}

/// <summary>
/// An outcome of an activity.
/// </summary>
public sealed class WorkflowDesignerOutcome
{
    /// <summary>
    /// The outcome name, used by transitions.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The localized outcome name.
    /// </summary>
    public string DisplayName { get; init; }
}

/// <summary>
/// A transition between two activities.
/// </summary>
public sealed class WorkflowDesignerTransition
{
    /// <summary>
    /// The source <see cref="ActivityRecord.ActivityId"/>.
    /// </summary>
    public string SourceActivityId { get; set; }

    /// <summary>
    /// The outcome of the source activity.
    /// </summary>
    public string SourceOutcomeName { get; set; }

    /// <summary>
    /// The destination <see cref="ActivityRecord.ActivityId"/>.
    /// </summary>
    public string DestinationActivityId { get; set; }

    internal static WorkflowDesignerTransition From(Transition transition)
        => new()
        {
            SourceActivityId = transition.SourceActivityId,
            SourceOutcomeName = transition.SourceOutcomeName,
            DestinationActivityId = transition.DestinationActivityId,
        };

    internal Transition ToTransition()
        => new()
        {
            SourceActivityId = SourceActivityId,
            SourceOutcomeName = SourceOutcomeName,
            DestinationActivityId = DestinationActivityId,
        };
}
