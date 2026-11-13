using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Suggests the alternates of the shapes that can be overridden from the Templates and Admin Templates lists,
/// e.g. the alternates of the content items, parts and fields of the content types.
/// </summary>
public interface ITemplateAlternatesProvider
{
    /// <summary>
    /// Lists the alternates the provider knows of.
    /// </summary>
    /// <param name="adminTemplates">Whether to list the alternates rendered by the admin, rather than by the front end.</param>
    Task<IEnumerable<TemplateAlternate>> GetAlternatesAsync(bool adminTemplates);
}
