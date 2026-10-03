namespace OrchardCore.Secrets.ViewModels;

public class SecretsDeploymentStepViewModel
{
    public string EncryptionKeyName { get; set; }
    public IEnumerable<string> AvailableEncryptionKeys { get; set; } = [];
}
