using OrchardCore.Localization;

namespace OrchardCore.Workflows.Activities;

/// <summary>
/// Describes an activity with static members, so its name and category are declared once, without a localizer, and
/// read without creating the activity. An activity that implements it returns their translations from
/// <see cref="IActivity.DisplayText"/> and <see cref="IActivity.Category"/>, for example with
/// <c>IStringLocalizerFactory.Localize(DisplayName)</c>.
/// </summary>
public interface IActivityDescription
{
    /// <summary>
    /// Gets the name of the activity shown to users, for example in the designer's activities pane.
    /// </summary>
    static abstract LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the category the activity is listed under. Its untranslated value also picks the default icon of the
    /// activity in the designer.
    /// </summary>
    static abstract LocalizationSource CategoryName { get; }
}
