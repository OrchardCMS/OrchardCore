namespace OrchardCore.Security.Options
{
    public class SandboxContentSecurityPolicyOptions : ContentSecurityPolicyOptionsBase
    {
        internal static readonly string Separator = " ";

        public override string Name => ContentSecurityPolicyValue.Sandbox;

        public override string ToString() => Name + Separator + Value.TrimStart();
    }
}
