namespace OrchardCore.Localization;

/// <summary>
/// Untranslated strings that have the same meaning across modules, such as the categories of deployment steps.
/// </summary>
/// <remarks>
/// Reuse these sources instead of declaring the same text in each module, so that it is translated once.
/// Their PO context is <c>OrchardCore.Localization.CommonLocalizationSources</c>. Declare the strings
/// that are specific to a module in an internal class of the module, and keep the strings that are
/// specific to a view in the view, with its localizer.
/// </remarks>
public static class CommonLocalizationSources
{
    /// <summary>Gets the "Configuration" text.</summary>
    public static readonly LocalizationSource Configuration = LocalizationSource.Create("Configuration", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Content" text.</summary>
    public static readonly LocalizationSource Content = LocalizationSource.Create("Content", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Content Management" text.</summary>
    public static readonly LocalizationSource ContentManagement = LocalizationSource.Create("Content Management", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Data" text.</summary>
    public static readonly LocalizationSource Data = LocalizationSource.Create("Data", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Deployment" text.</summary>
    public static readonly LocalizationSource Deployment = LocalizationSource.Create("Deployment", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Development" text.</summary>
    public static readonly LocalizationSource Development = LocalizationSource.Create("Development", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Indexing" text.</summary>
    public static readonly LocalizationSource Indexing = LocalizationSource.Create("Indexing", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Infrastructure" text.</summary>
    public static readonly LocalizationSource Infrastructure = LocalizationSource.Create("Infrastructure", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Internationalization" text.</summary>
    public static readonly LocalizationSource Internationalization = LocalizationSource.Create("Internationalization", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Search" text.</summary>
    public static readonly LocalizationSource Search = LocalizationSource.Create("Search", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Security" text.</summary>
    public static readonly LocalizationSource Security = LocalizationSource.Create("Security", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Settings" text.</summary>
    public static readonly LocalizationSource Settings = LocalizationSource.Create("Settings", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Theming" text.</summary>
    public static readonly LocalizationSource Theming = LocalizationSource.Create("Theming", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Workflows" text.</summary>
    public static readonly LocalizationSource Workflows = LocalizationSource.Create("Workflows", typeof(CommonLocalizationSources));
}
