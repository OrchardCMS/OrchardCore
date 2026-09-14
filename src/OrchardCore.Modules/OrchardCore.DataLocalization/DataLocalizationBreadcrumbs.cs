namespace OrchardCore.DataLocalization;

/// <summary>
/// The names of the breadcrumbs rendered by the dynamic translations screens. A module adds a node to one of these
/// trails by registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class DataLocalizationBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the dynamic translations screen.
    /// </summary>
    public const string List = "DynamicTranslations";

    /// <summary>
    /// The breadcrumb of the dynamic translation statistics screen.
    /// </summary>
    public const string Statistics = "DynamicTranslationsStatistics";
}
