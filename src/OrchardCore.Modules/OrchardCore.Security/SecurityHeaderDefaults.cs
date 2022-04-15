namespace OrchardCore.Security
{
    public static class SecurityHeaderDefaults
    {
        // TODO: Set the default security headers values
        public static readonly string ReferrerPolicy = ReferrerPolicyOptions.NoReferrer;

        public static readonly string FrameOptions = Security.FrameOptions.SameOrigin;
    }
}
