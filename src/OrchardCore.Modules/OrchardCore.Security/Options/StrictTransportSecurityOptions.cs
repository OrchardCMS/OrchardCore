using System;

namespace OrchardCore.Security.Options
{
    public class StrictTransportSecurityOptions
    {
        public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(365);

        public bool IncludeSubDomains { get; set; } = true;

        public bool Preload { get; set; }
    }
}
