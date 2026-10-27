using Microsoft.Extensions.Localization;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Lists shape templates shipped by extensions, e.g. the active theme, so that they can be viewed and overridden
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
    /// Lists the templates of the source.
    /// </summary>
    /// <param name="adminTemplates">Whether to list the templates used by the admin, rather than by the front end.</param>
    Task<IEnumerable<SourceTemplate>> GetTemplatesAsync(bool adminTemplates);
}
