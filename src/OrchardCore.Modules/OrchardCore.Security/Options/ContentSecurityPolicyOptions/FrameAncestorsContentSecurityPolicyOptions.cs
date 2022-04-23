namespace OrchardCore.Security.Options
{
    public class FrameAncestorsContentSecurityPolicyOptions : ContentSecurityPolicyOptionsBase
    {
        public override string Name => ContentSecurityPolicyValue.FrameAncestors;

        public string Origin { get; set; } = ContentSecurityPolicyOriginValue.None;

        public string[] AllowedOrigins { get; set; }
    }
}
