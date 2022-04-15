namespace OrchardCore.Security
{
    public class SecuritySettings
    {
        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;

        public string FrameOptions { get; set; } = SecurityHeaderDefaults.FrameOptions;
    }
}
