namespace OrchardCore.Layers;

/// <summary>
/// The names of the breadcrumbs rendered by the layers screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class LayersBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the widgets and layers screen. It is named after the list of that screen.
    /// </summary>
    public const string List = "Layers";

    /// <summary>
    /// The breadcrumb of the layer creation screen.
    /// </summary>
    public const string Create = "LayersCreate";

    /// <summary>
    /// The breadcrumb of the layer edition screen. It carries <see cref="LayerNameKey"/>.
    /// </summary>
    public const string Edit = "LayersEdit";

    /// <summary>
    /// The breadcrumb of the layer rule creation screen.
    /// </summary>
    public const string RuleCreate = "LayersRuleCreate";

    /// <summary>
    /// The breadcrumb of the layer rule edition screen.
    /// </summary>
    public const string RuleEdit = "LayersRuleEdit";

    /// <summary>
    /// The key under which the edition screen passes the name of the layer it is about.
    /// </summary>
    public const string LayerNameKey = "LayerName";
}
