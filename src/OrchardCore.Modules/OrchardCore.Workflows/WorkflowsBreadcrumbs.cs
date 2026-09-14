namespace OrchardCore.Workflows;

/// <summary>
/// The names of the breadcrumbs rendered by the workflows screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class WorkflowsBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the workflow types list. It is named after the list of that screen.
    /// </summary>
    public const string TypesList = "WorkflowTypes";

    /// <summary>
    /// The breadcrumb of the workflow designer. It carries <see cref="TypeNameKey"/> and <see cref="TypeIdKey"/>.
    /// </summary>
    public const string TypeEdit = "WorkflowTypesEdit";

    /// <summary>
    /// The breadcrumb of the workflow properties screen.
    /// </summary>
    public const string TypeEditProperties = "WorkflowTypesEditProperties";

    /// <summary>
    /// The breadcrumb of the workflow duplication screen.
    /// </summary>
    public const string TypeDuplicate = "WorkflowTypesDuplicate";

    /// <summary>
    /// The breadcrumb of the workflow instances list. It is named after the list of that screen. It carries
    /// <see cref="TypeNameKey"/> and <see cref="TypeIdKey"/>, and it leads back to the workflow the instances belong to.
    /// </summary>
    public const string Instances = "WorkflowInstances";

    /// <summary>
    /// The breadcrumb of the workflow instance screen. It carries <see cref="TypeNameKey"/>, <see cref="TypeIdKey"/>
    /// and <see cref="WorkflowIdKey"/>.
    /// </summary>
    public const string InstanceDetails = "WorkflowInstancesDetails";

    /// <summary>
    /// The breadcrumb of the activity creation screen. It carries <see cref="ActivityNameKey"/>.
    /// </summary>
    public const string ActivityCreate = "WorkflowsActivityCreate";

    /// <summary>
    /// The breadcrumb of the activity edition screen. It carries <see cref="ActivityNameKey"/>.
    /// </summary>
    public const string ActivityEdit = "WorkflowsActivityEdit";

    /// <summary>
    /// The key under which a screen passes the name of the workflow type it is about.
    /// </summary>
    public const string TypeNameKey = "TypeName";

    /// <summary>
    /// The key under which a screen passes the identifier of the workflow type it is about.
    /// </summary>
    public const string TypeIdKey = "TypeId";

    /// <summary>
    /// The key under which a screen passes the identifier of the workflow instance it is about.
    /// </summary>
    public const string WorkflowIdKey = "WorkflowId";

    /// <summary>
    /// The key under which a screen passes the display name of the activity it is about.
    /// </summary>
    public const string ActivityNameKey = "ActivityName";
}
