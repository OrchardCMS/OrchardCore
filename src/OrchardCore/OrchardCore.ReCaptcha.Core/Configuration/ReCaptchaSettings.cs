namespace OrchardCore.ReCaptcha.Configuration;

public class ReCaptchaSettings
{
    public string SiteKey { get; set; }

    public string SecretKey { get; set; }

    public string ReCaptchaScriptUri { get; set; } = Constants.ReCaptchaScriptUri;

    public string ReCaptchaApiUri { get; set; } = Constants.ReCaptchaApiUri;

    private bool? _configurationExists;

    public bool ConfigurationExists()
        => _configurationExists ??= !string.IsNullOrWhiteSpace(SiteKey)
        && !string.IsNullOrWhiteSpace(SecretKey)
        && !string.IsNullOrWhiteSpace(ReCaptchaApiUri);

    [Obsolete($"This method is obsolete and will be removed in future releases. Instead use {nameof(ConfigurationExists)}.")]
    public bool IsValid()
        => ConfigurationExists();
}
