namespace OrchardCore.Email.Smtp.Secrets;

/// <summary>
/// Settings for SMTP credentials stored in secrets.
/// </summary>
public class SmtpSecretSettings
{
    /// <summary>
    /// Gets or sets the name of the secret containing the SMTP password.
    /// When set, this takes precedence over the password stored in SmtpSettings.
    /// </summary>
    public string PasswordSecretName { get; set; }
}
