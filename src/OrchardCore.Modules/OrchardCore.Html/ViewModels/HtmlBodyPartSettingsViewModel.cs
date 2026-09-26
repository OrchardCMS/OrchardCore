using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Html.ViewModels;

public class HtmlBodyPartSettingsViewModel
{
    public bool SanitizeHtml { get; set; }
    public bool RenderLiquid { get; set; }

    [BindNever]
    public bool CanManageLiquidTemplates { get; set; }
}
