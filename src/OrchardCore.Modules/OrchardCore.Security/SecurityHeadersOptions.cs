namespace OrchardCore.Security
{
    public class SecurityHeadersOptions
    {
        public ReferrerPolicy ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;
    }
}
