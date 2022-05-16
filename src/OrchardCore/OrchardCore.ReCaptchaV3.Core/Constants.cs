namespace OrchardCore.ReCaptchaV3
{
    public static class Constants
    {
        public const string ReCaptchaV3ScriptUri = "https://www.google.com/recaptcha/api.js";

        public const string ReCaptchaV3ApiUri = "https://www.google.com/recaptcha/api/";

        public const string ReCaptchaV3ServerResponseHeaderName = "g-recaptcha-response";

        public const decimal ReCaptchaV3DefaultThreshold = 0.5m;
    }
}
