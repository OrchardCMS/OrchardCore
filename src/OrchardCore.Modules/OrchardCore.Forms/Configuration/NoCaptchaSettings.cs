namespace OrchardCore.Forms.Configuration
{
    public class NoCaptchaSettings
    {
        public string SiteKey { get; set; }
        public string SiteSecret { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SiteKey) && !string.IsNullOrWhiteSpace(SiteSecret);
        }
    }
}
