namespace OrchardCore.ReCaptcha.ViewModels
{
    public class ReCaptchaSettingsViewModel
    {
        public string SiteKey { get; set; }

        public string SecretKey { get; set; }

        public string ReCaptchaScriptType { get; set; }

        public string ReCaptchaScriptClass { get; set; }
    }
}
