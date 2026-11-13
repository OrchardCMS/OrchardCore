using OrchardCore.Localization;

namespace OrchardCore.Workflows.Activities;

/// <summary>
/// The categories of the built-in activities, shared by the modules that provide activities, for
/// <see cref="IActivityDescription.CategoryName"/>. Their PO context is
/// <c>OrchardCore.Workflows.Activities.ActivityCategoryLocalizationSources</c>, and the untranslated
/// <see cref="LocalizationSource.Value"/> of a category is its key in <c>WorkflowOptions.CategoryIcons</c>.
/// </summary>
public static class ActivityCategoryLocalizationSources
{
    /// <summary>Gets the "Background" category, of the activities that run outside of a request.</summary>
    public static readonly LocalizationSource Background = LocalizationSource.Create("Background", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "Content" category, of the activities about content items.</summary>
    public static readonly LocalizationSource Content = LocalizationSource.Create("Content", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "Control Flow" category, of the activities that branch, loop or run scripts.</summary>
    public static readonly LocalizationSource ControlFlow = LocalizationSource.Create("Control Flow", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "Exceptions" category.</summary>
    public static readonly LocalizationSource Exceptions = LocalizationSource.Create("Exceptions", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "HTTP" category, of the activities about HTTP requests and responses.</summary>
    public static readonly LocalizationSource Http = LocalizationSource.Create("HTTP", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "Messaging" category, of the activities that send messages such as emails.</summary>
    public static readonly LocalizationSource Messaging = LocalizationSource.Create("Messaging", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "Primitives" category, of the activities that set values or run other workflows.</summary>
    public static readonly LocalizationSource Primitives = LocalizationSource.Create("Primitives", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "Session" category, of the activities about the database session.</summary>
    public static readonly LocalizationSource Session = LocalizationSource.Create("Session", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "UI" category, of the activities that show something to the user.</summary>
    public static readonly LocalizationSource UI = LocalizationSource.Create("UI", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "User" category, of the activities about users.</summary>
    public static readonly LocalizationSource User = LocalizationSource.Create("User", typeof(ActivityCategoryLocalizationSources));

    /// <summary>Gets the "Validation" category, of the activities that validate a request or a form.</summary>
    public static readonly LocalizationSource Validation = LocalizationSource.Create("Validation", typeof(ActivityCategoryLocalizationSources));
}
