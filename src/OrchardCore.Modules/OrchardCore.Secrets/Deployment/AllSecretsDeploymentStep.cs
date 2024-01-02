using OrchardCore.Deployment;

namespace OrchardCore.Secrets.Deployment;

public class AllSecretsDeploymentStep : DeploymentStep
{
    public AllSecretsDeploymentStep()
    {
        Name = "AllSecretsDeploymentStep";
    }
}
