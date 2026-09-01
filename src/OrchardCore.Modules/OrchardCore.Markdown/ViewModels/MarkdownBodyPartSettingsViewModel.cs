using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Markdown.ViewModels;

public class MarkdownBodyPartSettingsViewModel
{
    public bool SanitizeHtml { get; set; }
    public bool RenderLiquid { get; set; }

    [BindNever]
    public bool CanManageLiquidTemplates { get; set; }
}
