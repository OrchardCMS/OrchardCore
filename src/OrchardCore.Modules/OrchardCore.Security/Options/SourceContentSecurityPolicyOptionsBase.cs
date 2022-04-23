namespace OrchardCore.Security.Options
{
    public abstract class SourceContentSecurityPolicyOptionsBase : ContentSecurityPolicyOptionsBase
    {
        public string Origin { get; set; } = ContentSecurityPolicyOriginValue.None;

        public string[] AllowedOrigins { get; set; }
    }
}
