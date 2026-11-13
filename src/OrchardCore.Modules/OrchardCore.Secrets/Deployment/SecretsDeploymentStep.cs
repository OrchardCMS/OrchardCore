using OrchardCore.Deployment;

namespace OrchardCore.Secrets.Deployment;

/// <summary>
/// Adds all secrets to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class SecretsDeploymentStep : DeploymentStep
{
    public SecretsDeploymentStep()
    {
        Name = "Secrets";
    }

    /// <summary>
    /// Gets or sets the name of the encryption key (RsaKeySecret or X509Secret) to use for encrypting secrets.
    /// When set, secret values are exported encrypted. When null/empty, only metadata is exported.
    /// </summary>
    public string EncryptionKeyName { get; set; }
}
