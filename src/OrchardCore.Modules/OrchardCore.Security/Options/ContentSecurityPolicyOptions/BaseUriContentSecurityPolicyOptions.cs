namespace OrchardCore.Security.Options
{
    public class BaseUriContentSecurityPolicyOptions : SourceContentSecurityPolicyOptionsBase
    {
        public override string Name => ContentSecurityPolicyValue.BaseUri;
    }
}
