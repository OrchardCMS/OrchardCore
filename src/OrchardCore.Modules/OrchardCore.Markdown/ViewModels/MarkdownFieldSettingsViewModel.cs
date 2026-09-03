using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Markdown.ViewModels;

public class MarkdownFieldSettingsViewModel
{
    public bool SanitizeHtml { get; set; }
    public bool RenderLiquid { get; set; }
    public string Hint { get; set; }

    [BindNever]
    public bool CanManageLiquidTemplates { get; set; }
}
