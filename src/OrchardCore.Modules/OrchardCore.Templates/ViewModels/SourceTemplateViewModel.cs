using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.ViewModels;

public class SourceTemplateViewModel
{
    /// <summary>
    /// The template source of the template.
    /// </summary>
    public ITemplateSource Source { get; set; }

    /// <summary>
    /// The template and the state of its local override.
    /// </summary>
    public SourceTemplateEntry Entry { get; set; }

    /// <summary>
    /// The data context for rendering the alternates of the template, on the alternates page.
    /// </summary>
    public OrchardCore.Admin.AdminListContext Alternates { get; set; }

    /// <summary>
    /// The search of the alternates, on the alternates page.
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// The content of the source template.
    /// </summary>
    public string Content { get; set; }
}
