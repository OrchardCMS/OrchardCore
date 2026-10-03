namespace OrchardCore.Secrets.Azure;

public enum AzureKeyVaultCredentialType
{
    ManagedIdentity,
    WorkloadIdentity,
    ClientSecret,
    AzureCli,
    AzurePowerShell,
    VisualStudio,
    DefaultAzureCredential,
}
