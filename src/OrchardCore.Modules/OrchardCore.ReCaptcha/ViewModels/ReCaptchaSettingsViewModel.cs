namespace OrchardCore.ReCaptcha.ViewModels
{
    public class ReCaptchaSettingsViewModel
    {
        public string SiteKey { get; set; }

        public string SecretKey { get; set; }

        public bool HardenLoginProcess { get; set; }

        public bool HardenForgotPasswordProcess { get; set; }

        public bool HardenRegisterProcess { get; set; }
    }
}
