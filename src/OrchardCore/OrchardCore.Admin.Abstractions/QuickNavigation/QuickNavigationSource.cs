namespace OrchardCore.Admin.QuickNavigation;

/// <summary>
/// Supplies quick navigation entries and renders them for the current user and culture.
/// Register sources as scoped services from their owning, opt-in features.
/// Sources that need the current user can inject <see cref="Microsoft.AspNetCore.Http.IHttpContextAccessor"/>.
/// </summary>
public abstract class QuickNavigationSource
{
    /// <summary>
    /// Gets the unique, culture-independent name used to route indexed entries to this source.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets request-specific entry identifiers to supplement this source's entries in
    /// <see cref="IQuickNavigationIndex"/>. Override this for sources that cannot be indexed globally.
    /// Called on every index request. Sources may cache reusable identifiers when they can
    /// reliably invalidate them; the controller does not cache source execution or rendered results.
    /// </summary>
    public virtual ValueTask<IEnumerable<string>> GetEntryIdsAsync()
        => ValueTask.FromResult<IEnumerable<string>>([]);

    /// <summary>
    /// Authorizes and renders an entry in the current culture. Returns <see langword="null"/>
    /// if the entry no longer exists or the current user cannot access it.
    /// Never cache the returned value across users or cultures.
    /// </summary>
    /// <param name="entryId">The culture-independent identifier supplied by this source.</param>
    public abstract ValueTask<QuickNavigationResult> DisplayAsync(string entryId);
}
