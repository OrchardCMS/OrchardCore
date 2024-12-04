using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Layers.ViewModels;

public class LayerMetadataEditViewModel
{
    public string Title { get; set; }

    public bool RenderTitle { get; set; }

    public double Position { get; set; }

    public string Zone { get; set; }

    public string Layer { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Layers { get; set; }
}
