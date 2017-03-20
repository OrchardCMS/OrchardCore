using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace OrchardCore.Extensions
{
    public class ManifestOptions
    {
        public IList<ManifestOption> ManifestConfigurations { get; }
            = new List<ManifestOption>();
    }
}