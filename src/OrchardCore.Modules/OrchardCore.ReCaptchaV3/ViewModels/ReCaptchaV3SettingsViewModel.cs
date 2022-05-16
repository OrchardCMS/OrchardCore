namespace OrchardCore.ReCaptchaV3.ViewModels
{
    public class ReCaptchaV3SettingsViewModel
    {
        public string SiteKey { get; set; }

        public string SecretKey { get; set; }

        public decimal Threshold { get; set; }
    }
}
