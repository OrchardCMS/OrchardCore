namespace OrchardCore.Azure.Core;

public enum AzureAuthenticationType
{
    // Token Credential Types
    Default,
    ManagedIdentity,
    AzureCli,
    VisualStudio,
    VisualStudioCode,
    AzurePowerShell,
    Environment,
    InteractiveBrowser,
    WorkloadIdentity,

    // Non-token Credential Types
    ApiKey,
}
