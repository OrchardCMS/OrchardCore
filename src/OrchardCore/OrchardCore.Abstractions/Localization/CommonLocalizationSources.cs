namespace OrchardCore.Localization;

/// <summary>
/// Untranslated strings that several modules use with the same meaning, such as the categories of deployment steps
/// that they share, or that are general to every module.
/// </summary>
/// <remarks>
/// Reuse these sources instead of declaring the same text in each module, so that it is translated once. Add a
/// source only when it is used by more than one project, or is general to every module, such as "Settings".
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

    /// <summary>Gets the "Development" text.</summary>
    public static readonly LocalizationSource Development = LocalizationSource.Create("Development", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Infrastructure" text.</summary>
    public static readonly LocalizationSource Infrastructure = LocalizationSource.Create("Infrastructure", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Search" text.</summary>
    public static readonly LocalizationSource Search = LocalizationSource.Create("Search", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Security" text.</summary>
    public static readonly LocalizationSource Security = LocalizationSource.Create("Security", typeof(CommonLocalizationSources));

    /// <summary>Gets the "Settings" text.</summary>
    public static readonly LocalizationSource Settings = LocalizationSource.Create("Settings", typeof(CommonLocalizationSources));
}
