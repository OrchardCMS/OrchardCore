namespace OrchardCore.Shortcodes;

/// <summary>
/// The names of the breadcrumbs rendered by the shortcodes screens. A module adds a node to one of these trails by
/// registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class ShortcodesConstants
{
    /// <summary>
    /// The breadcrumb of the shortcodes list. It is named after the list of that screen.
    /// </summary>
    public const string List = "Shortcodes";

    /// <summary>
    /// The breadcrumb of the shortcode creation screen.
    /// </summary>
    public const string Create = "ShortcodesCreate";

    /// <summary>
    /// The breadcrumb of the shortcode edition screen.
    /// </summary>
    public const string Edit = "ShortcodesEdit";
}
