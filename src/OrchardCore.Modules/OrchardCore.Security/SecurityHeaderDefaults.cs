using System.Collections.Generic;
using OrchardCore.Security.Options;

namespace OrchardCore.Security
{
    public static class SecurityHeaderDefaults
    {
        public static readonly string ContentTypeOptions = ContentTypeOptionsValue.NoSniff;

        public static readonly string FrameOptions = FrameOptionsValue.SameOrigin;

        public static readonly ICollection<string> PermissionsPolicy = new [] { "fullscreen" };

        public static readonly string ReferrerPolicy = ReferrerPolicyValue.NoReferrer;
    }
}
