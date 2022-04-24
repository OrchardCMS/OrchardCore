namespace OrchardCore.Security.Options
{
    public class UpgradeInsecureRequestsContentSecurityPolicyOptions : ContentSecurityPolicyOptionsBase
    {
        public override string Name => ContentSecurityPolicyValue.UpgradeInsecureRequests;

        public override string ToString() => Name;
    }
}
