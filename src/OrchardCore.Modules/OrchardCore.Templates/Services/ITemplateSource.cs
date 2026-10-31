using Microsoft.Extensions.Localization;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Lists shape templates shipped by extensions, e.g. the themes, so that they can be viewed and overridden
/// from the Templates and Admin Templates lists. Each source is a button above these lists.
/// </summary>
public interface ITemplateSource
{
    /// <summary>
    /// Gets the technical name of the source, used in its URL, e.g. <c>Theme</c>.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the name of the source shown to the user, e.g. "Theme Templates".
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Describes the templates of the source, shown above its list.
    /// </summary>
    /// <param name="adminTemplates">Whether the list shows the admin templates.</param>
    LocalizedString GetDescription(bool adminTemplates);

    /// <summary>
    /// Lists the parts of the source that can be listed on their own, e.g. the themes, which the user picks from.
    /// The first one is listed by default.
    /// </summary>
    /// <param name="adminTemplates">Whether to list the scopes used by the admin, rather than by the front end.</param>
    /// <returns>The scopes, or none when the source lists its templates at once.</returns>
    Task<IEnumerable<TemplateSourceScope>> GetScopesAsync(bool adminTemplates);

    /// <summary>
    /// Lists the templates of the source.
    /// </summary>
    /// <param name="adminTemplates">Whether to list the templates used by the admin, rather than by the front end.</param>
    /// <param name="scope">The name of the scope to list, or <see langword="null"/> for the default one.</param>
    Task<IEnumerable<SourceTemplate>> GetTemplatesAsync(bool adminTemplates, string scope);
}
