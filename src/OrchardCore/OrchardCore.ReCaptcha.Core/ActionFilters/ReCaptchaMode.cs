namespace OrchardCore.ReCaptcha.ActionFilters
{
    public enum ReCaptchaMode
    {
        /// <summary>
        /// Captcha is always shown.
        /// </summary>
        AlwaysShow,

        /// <summary>
        /// Captcha initially is not shown, but when a robot is detected it will show on until dismissed.
        /// </summary>
        PreventRobots
    }
}
