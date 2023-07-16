using System;

namespace OrchardCore.ReCaptcha.Configuration
{
    public class ReCaptchaSettings
    {
        public string SiteKey { get; set; }

        public string SecretKey { get; set; }

        public string ReCaptchaScriptUri { get; set; } = Constants.ReCaptchaScriptUri;

        public string ReCaptchaApiUri { get; set; } = Constants.ReCaptchaApiUri;

        /// <summary>
        /// Value for supplying the amount of lenience we are willing to show robots.
        /// </summary>
        public int DetectionThreshold { get; set; } = 5;

        public bool IsValid()
        {
            return !String.IsNullOrWhiteSpace(SiteKey) && !String.IsNullOrWhiteSpace(SecretKey);
        }
    }
}
