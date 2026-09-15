namespace OrchardCore.Templates;

/// <summary>
/// The names of the breadcrumbs rendered by the templates screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class TemplatesConstants
{
    /// <summary>
    /// The breadcrumb of the templates list. It is named after the list of that screen. It carries
    /// <see cref="AdminTemplatesKey"/>.
    /// </summary>
    public const string List = "Templates";

    /// <summary>
    /// The breadcrumb of the template creation screen. It carries <see cref="AdminTemplatesKey"/>.
    /// </summary>
    public const string Create = "TemplatesCreate";

    /// <summary>
    /// The breadcrumb of the template edition screen. It carries <see cref="AdminTemplatesKey"/>.
    /// </summary>
    public const string Edit = "TemplatesEdit";

    /// <summary>
    /// The key under which a screen says whether it is about the templates of the admin rather than the templates of
    /// the site, as a <c>bool</c>. The two are separate screens sharing one set of views.
    /// </summary>
    public const string AdminTemplatesKey = "AdminTemplates";
}
