namespace OrchardCore.Security.Options
{
    public abstract class ContentSecurityPolicyOptionsBase
    {
        public abstract string Name { get; }

        public string Value { get; set; }
    }
}
