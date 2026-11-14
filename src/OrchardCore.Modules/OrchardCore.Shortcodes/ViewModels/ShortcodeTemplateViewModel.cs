using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Shortcodes.ViewModels;

public class ShortcodeTemplateViewModel
{
    public string Name { get; set; }
    public string Content { get; set; }
    public string Hint { get; set; }
    public string Usage { get; set; }
    public string DefaultValue { get; set; }
    public string[] Categories { get; set; } = [];
    public string SelectedCategories { get; set; }

    /// <summary>
    /// Gets or sets the editor of the shortcode template, built by the display drivers of <see cref="ShortcodeTemplateViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
