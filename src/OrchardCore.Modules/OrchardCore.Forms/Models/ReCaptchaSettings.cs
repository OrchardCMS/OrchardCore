using System;

namespace OrchardCore.Forms.Models
{
    public class ReCaptchaSettings
    {
        public string SiteKey { get; set; }
        public string SiteSecret { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SiteKey) && !string.IsNullOrWhiteSpace(SiteSecret);
        }
    }
}
