using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Json;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Deployment;

public sealed class AllSitemapsDeploymentSource
    : DeploymentSourceBase<AllSitemapsDeploymentStep>
{
    private readonly ISitemapManager _sitemapManager;
    private readonly DocumentJsonSerializerOptions _documentJsonSerializerOptions;

    public AllSitemapsDeploymentSource(
        ISitemapManager sitemapManager,
        IOptions<DocumentJsonSerializerOptions> documentJsonSerializerOptions)
    {
        _sitemapManager = sitemapManager;
        _documentJsonSerializerOptions = documentJsonSerializerOptions.Value;
    }

    protected override async Task ProcessAsync(AllSitemapsDeploymentStep step, DeploymentPlanResult result)
    {
        var sitemaps = await _sitemapManager.GetSitemapsAsync();

        var jArray = JArray.FromObject(sitemaps, _documentJsonSerializerOptions.SerializerOptions);

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Sitemaps",
            ["data"] = jArray,
        });
    }
}
