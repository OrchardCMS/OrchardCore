using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Templates.ViewModels;

public class TemplateViewModel
{
    public bool AdminTemplates { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the editor of the template, built by the display drivers of <see cref="TemplateViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
