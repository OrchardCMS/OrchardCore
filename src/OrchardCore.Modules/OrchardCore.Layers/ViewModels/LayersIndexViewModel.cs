using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.ViewModels;

public class LayersIndexViewModel
{
    [BindNever]
    public string[] Zones { get; set; }

    [BindNever]
    public Dictionary<string, List<dynamic>> Widgets { get; set; } = [];

    [BindNever]
    public List<Layer> Layers { get; set; }
}
