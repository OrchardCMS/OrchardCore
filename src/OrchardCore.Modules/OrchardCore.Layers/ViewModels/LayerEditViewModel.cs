using System.Collections.Generic;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Layers.ViewModels
{
    public class LayerEditViewModel
    {
        public string Name { get; set; }
        public string Rule { get; set; }
        public string Description { get; set; }
        public dynamic RuleContainer { get; set; }
        public Dictionary<string, dynamic> Thumbnails { get; set; }
    }
}
