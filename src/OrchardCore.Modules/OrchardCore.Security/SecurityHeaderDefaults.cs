using System.Collections.Generic;

namespace OrchardCore.Security
{
    public static class SecurityHeaderDefaults
    {
        // TODO: Set the default security headers values
        public static readonly ContentTypeOptionsValue ContentTypeOptions = ContentTypeOptionsValue.NoSniff;

        public static readonly FrameOptionsValue FrameOptions = FrameOptionsValue.SameOrigin;

        public static readonly ICollection<string> PermissionsPolicy = new List<string>();

        public static readonly ReferrerPolicyValue ReferrerPolicy = ReferrerPolicyValue.NoReferrer;
    }
}
