namespace OrchardCore.Security
{
    public class SecuritySettings
    {
        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;

        public string XFrameOptions { get; set; } = SecurityHeaderDefaults.FrameOptions;
    }
}
