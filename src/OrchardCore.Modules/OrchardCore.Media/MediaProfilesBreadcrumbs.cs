namespace OrchardCore.Media;

/// <summary>
/// The names of the breadcrumbs rendered by the media profiles and media cache screens. A module adds a node to one of
/// these trails by registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class MediaProfilesBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the media profiles list. It is named after the list of that screen.
    /// </summary>
    public const string List = "MediaProfiles";

    /// <summary>
    /// The breadcrumb of the media profile creation screen.
    /// </summary>
    public const string Create = "MediaProfilesCreate";

    /// <summary>
    /// The breadcrumb of the media profile edition screen.
    /// </summary>
    public const string Edit = "MediaProfilesEdit";

    /// <summary>
    /// The breadcrumb of the media cache screen.
    /// </summary>
    public const string Cache = "MediaCache";
}
