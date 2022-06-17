using System.Collections.Generic;
using OrchardCore.Features.Models;

namespace OrchardCore.Features.ViewModels
{
    public class FeaturesViewModel
    {
        public string Name { get; set; }

        public IEnumerable<ModuleFeature> Features { get; set; }
    }
}
