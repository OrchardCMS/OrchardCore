using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Secrets.Deployment;

public sealed class SecretsDeploymentSource : DeploymentSourceBase<SecretsDeploymentStep>
{
    private readonly ISecretManager _secretManager;

    public SecretsDeploymentSource(ISecretManager secretManager)
    {
        _secretManager = secretManager;
    }

    protected override async Task ProcessAsync(SecretsDeploymentStep step, DeploymentPlanResult result)
    {
        var secretInfos = await _secretManager.GetSecretInfosAsync();

        var secrets = new JsonArray();

        foreach (var info in secretInfos)
        {
            // Note: We only export metadata, not the actual secret values
            // Secret values must be provided during import via environment variables or other secure means
            secrets.Add(new JsonObject
            {
                ["Name"] = info.Name,
                ["Store"] = info.Store,
                ["Type"] = info.Type,
            });
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Secrets",
            ["Secrets"] = secrets,
        });
    }
}
