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
}
