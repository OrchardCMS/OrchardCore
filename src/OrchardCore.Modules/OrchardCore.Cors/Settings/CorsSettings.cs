using System.Collections.Generic;

namespace OrchardCore.Cors.Settings
{
    public class CorsSettings
    {
        public IEnumerable<CorsPolicySetting> Policies { get; set; }
    }
}
