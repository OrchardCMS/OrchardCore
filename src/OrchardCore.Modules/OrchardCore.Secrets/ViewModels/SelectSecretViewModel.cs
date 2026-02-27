using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Secrets.ViewModels;

public class SelectSecretViewModel
{
    public string HtmlId { get; set; }

    public string HtmlName { get; set; }

    public List<SelectListItem> Secrets { get; set; } = [];
}
