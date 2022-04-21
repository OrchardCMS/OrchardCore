using System;
using System.Collections.Generic;
using OrchardCore.Security.Options;

namespace OrchardCore.Security
{
    public static class SecurityHeaderDefaults
    {
        public static readonly ContentTypeOptionsValue ContentTypeOptions = ContentTypeOptionsValue.NoSniff;

        public static readonly FrameOptionsValue FrameOptions = FrameOptionsValue.SameOrigin;

        public static readonly ICollection<string> PermissionsPolicy = new [] { "fullscreen" };

        public static readonly ReferrerPolicyValue ReferrerPolicy = ReferrerPolicyValue.NoReferrer;

        public static readonly StrictTransportSecurityOptions StrictTransportSecurityOptions = new()
        {
            MaxAge = TimeSpan.FromDays(365),
            IncludeSubDomains = true
        };
    }
}
