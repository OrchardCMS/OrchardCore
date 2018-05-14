using System.Collections.Generic;

namespace OrchardCore.Environment.Extensions
{
    public class ManifestOptions 
    {
        public IList<ManifestOption> ManifestConfigurations { get; }
            = new List<ManifestOption>();
    }
}