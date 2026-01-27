namespace OrchardCore.Email.Smtp.Secrets.ViewModels;

public class SmtpSecretSettingsViewModel
{
    public string PasswordSecretName { get; set; }
    public IEnumerable<string> AvailableSecrets { get; set; } = [];
}
