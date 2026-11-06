namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// Lists tab and card names for their translation with data localization.
/// </summary>
/// <remarks>
/// Implement it for names that can't be discovered at build time, such as names that come from data or from stored
/// placement rules. The names of all contributors are listed once, under
/// <see cref="TabAndCardNames.DataLocalizationContext"/>.
/// </remarks>
public interface ITabAndCardNameContributor
{
    /// <summary>
    /// Gets the tab and card names.
    /// </summary>
    Task<IEnumerable<string>> GetNamesAsync();
}
