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
}
