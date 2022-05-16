namespace OrchardCore.ReCaptchaV3.Configuration
{
    public class ReCaptchaV3Settings
    {
        public string SiteKey { get; set; }

        public string SecretKey { get; set; }

        public string ReCaptchaV3ScriptUri { get; set; } = Constants.ReCaptchaV3ScriptUri;

        public string ReCaptchaV3ApiUri { get; set; } = Constants.ReCaptchaV3ApiUri;

        public decimal Threshold { get; set; } = Constants.ReCaptchaV3DefaultThreshold;

        public bool IsValid()
            => !string.IsNullOrWhiteSpace(SiteKey) && !string.IsNullOrWhiteSpace(SecretKey) && IsThresholdValid();

        public bool IsThresholdValid()
            => Threshold >= 0 && Threshold <= 1;
    }
}
