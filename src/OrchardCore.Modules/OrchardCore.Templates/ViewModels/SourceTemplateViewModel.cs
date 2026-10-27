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
    /// The content of the source template.
    /// </summary>
    public string Content { get; set; }
}
