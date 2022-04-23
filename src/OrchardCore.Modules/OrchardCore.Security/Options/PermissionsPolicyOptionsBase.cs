using System.Collections.Generic;

namespace OrchardCore.Security.Options
{
    public abstract class PermissionsPolicyOptionsBase
    {
        public abstract string Name { get; }

        public string Origin { get; set; } = PermissionsPolicyOriginValue.None;

        public string[] AllowedOrigins { get; set; }
    }
}
