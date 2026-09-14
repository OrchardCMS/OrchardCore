namespace OrchardCore.Deployment;

/// <summary>
/// The names of the breadcrumbs rendered by the deployment screens. A module adds a node to one of these trails by
/// registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class DeploymentBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the deployment plans list. It is named after the list of that screen.
    /// </summary>
    public const string List = "DeploymentPlans";

    /// <summary>
    /// The breadcrumb of the deployment plan creation screen.
    /// </summary>
    public const string Create = "DeploymentPlansCreate";

    /// <summary>
    /// The breadcrumb of the deployment plan edition screen.
    /// </summary>
    public const string Edit = "DeploymentPlansEdit";

    /// <summary>
    /// The breadcrumb of the deployment plan screen.
    /// </summary>
    public const string Display = "DeploymentPlansDisplay";

    /// <summary>
    /// The breadcrumb of the deployment step creation screen.
    /// </summary>
    public const string StepCreate = "DeploymentStepsCreate";

    /// <summary>
    /// The breadcrumb of the deployment step edition screen.
    /// </summary>
    public const string StepEdit = "DeploymentStepsEdit";

    /// <summary>
    /// The breadcrumb of the package import screen.
    /// </summary>
    public const string Import = "DeploymentImport";

    /// <summary>
    /// The breadcrumb of the json import screen.
    /// </summary>
    public const string ImportJson = "DeploymentImportJson";
}
