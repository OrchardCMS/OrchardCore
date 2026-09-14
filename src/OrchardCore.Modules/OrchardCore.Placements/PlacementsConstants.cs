namespace OrchardCore.Placements;

/// <summary>
/// The names of the breadcrumbs rendered by the placements screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class PlacementsConstants
{
    /// <summary>
    /// The breadcrumb of the placements list. It is named after the list of that screen.
    /// </summary>
    public const string List = "Placements";

    /// <summary>
    /// The breadcrumb of the placement edition screen, which also creates one. It carries <see cref="CreatingKey"/>.
    /// </summary>
    public const string Edit = "PlacementsEdit";

    /// <summary>
    /// The key under which the edition screen says whether it is creating a placement rather than editing one, as a
    /// <c>bool</c>.
    /// </summary>
    public const string CreatingKey = "Creating";
}
