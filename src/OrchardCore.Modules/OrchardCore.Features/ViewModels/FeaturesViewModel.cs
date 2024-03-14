using System.Collections.Generic;
using OrchardCore.Features.Models;

namespace OrchardCore.Features.ViewModels
{
    public class FeaturesViewModel
    {
        public string Name { get; set; }

        /// <summary>
        /// True when the current tenant is the Default one, and is executing on behalf of other tenant. Otherwise false.
        /// </summary>
        public bool IsProxy { get; set; }

        public IEnumerable<ModuleFeature> Features { get; set; }
    }
}
